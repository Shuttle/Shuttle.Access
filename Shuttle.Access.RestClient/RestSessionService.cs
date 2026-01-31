using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class RestSessionService(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, ISessionCache sessionCache, IAccessClient accessClient)
    : ISessionService, IContextSessionService
{
    private readonly AccessAuthorizationOptions _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
    private readonly ISessionCache _sessionCache = Guard.AgainstNull(sessionCache);
    private readonly IAccessClient _accessClient = Guard.AgainstNull(accessClient);
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<Messages.v1.Session?> FindAsync(CancellationToken cancellationToken = default)
    {
        var sessionResponse = await _accessClient.Sessions.GetSelfAsync(cancellationToken);

        var result = sessionResponse is { IsSuccessStatusCode: true, Content: not null } ? _sessionCache.Add(null, sessionResponse.Content) : null;

        if (result == null)
        {
            await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("Pass-Through", "(self)"), cancellationToken);
        }
        else
        {
            await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(result), cancellationToken);
        }

        return result;
    }

    public async Task<Messages.v1.Session?> FindAsync(Messages.v1.Session.Specification specification, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var session = _sessionCache.Find(specification);

            if (session != null)
            {
                await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(session), cancellationToken);

                return session;
            }

            if (_accessAuthorizationOptions.PassThrough)
            {
                return await FindAsync(cancellationToken);
            }

            specification.ShouldIncludePermissions = true;

            var sessionResponse = await _accessClient.Sessions.PostSearchAsync(specification, cancellationToken);

            if (sessionResponse is { IsSuccessStatusCode: true, Content: not null } && sessionResponse.Content.Any())
            {
                switch (sessionResponse.Content.Count())
                {
                    case 1:
                    {
                        var result = _sessionCache.Add(null, sessionResponse.Content.Single());

                        await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(result), cancellationToken);

                        return result;
                    }
                    case > 1:
                    {
                        throw new InvalidOperationException(string.Format(Access.Resources.SessionCountException));
                    }
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(specification.IdentityName))
                {
                    var registrationResponse = await _accessClient.Sessions.PostAsync(new RegisterSession
                    {
                        IdentityName = specification.IdentityName
                    }, cancellationToken);

                    var content = registrationResponse.Content;

                    if (!registrationResponse.IsSuccessStatusCode || content == null || !content.IsSuccessResult())
                    {
                        await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("IdentityName", specification.IdentityName), cancellationToken);

                        return null;
                    }

                    var result = new Messages.v1.Session
                    {
                        TenantId = content.TenantId,
                        IdentityId = content.IdentityId,
                        IdentityName = content.IdentityName,
                        DateRegistered = content.DateRegistered,
                        ExpiryDate = content.ExpiryDate,
                        Permissions = content.Permissions.ToList()
                    };

                    await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(result), cancellationToken);

                    return _sessionCache.Add(content.Token, result);
                }
            }

            await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("IdentityName", specification.IdentityName), cancellationToken);

            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddAsync(Guid? token, Messages.v1.Session session, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_sessionCache.Find(new()
                {
                    IdentityId = session.IdentityId
                }) != null)
            {
                return;
            }

            _sessionCache.Add(token, session);
        }
        finally
        {
            _lock.Release();
        }
    }
}