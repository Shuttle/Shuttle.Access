using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class RestSessionService(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, IAccessClient accessClient)
    : SessionCache, ISessionService, IContextSessionService
{
    private readonly AccessAuthorizationOptions _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
    private readonly IAccessClient _accessClient = Guard.AgainstNull(accessClient);
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<Messages.v1.Session?> FindAsync(CancellationToken cancellationToken = default)
    {
        var sessionResponse = await _accessClient.Sessions.GetSelfAsync(cancellationToken);

        var result = sessionResponse is { IsSuccessStatusCode: true, Content: not null } ? AddSession(null, sessionResponse.Content) : null;

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

    public async Task<Messages.v1.Session?> FindAsync(string identityName, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var session = Find(identityName);

            if (session != null)
            {
                await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(session), cancellationToken);

                return session;
            }

            if (_accessAuthorizationOptions.PassThrough)
            {
                return await FindAsync(cancellationToken);
            }

            var sessionResponse = await _accessClient.Sessions.PostSearchAsync(new()
            {
                IdentityName = identityName,
                ShouldIncludePermissions = true
            }, cancellationToken);

            if (sessionResponse is { IsSuccessStatusCode: true, Content: not null } && sessionResponse.Content.Any())
            {
                switch (sessionResponse.Content.Count())
                {
                    case 1:
                    {
                        var result = AddSession(null, sessionResponse.Content.Single());

                        await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(result), cancellationToken);

                        return result;
                    }
                    case > 1:
                    {
                        throw new InvalidOperationException(string.Format(Resources.UnexpectedMultipleSessionsException, "IdentityName", identityName));
                    }
                }
            }
            else
            {
                var registrationResponse = await _accessClient.Sessions.PostAsync(new RegisterSession
                {
                    IdentityName = identityName
                }, cancellationToken);

                var content = registrationResponse.Content;

                if (!registrationResponse.IsSuccessStatusCode ||
                    content == null ||
                    content.RegistrationRequested)
                {
                    await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("IdentityName", identityName), cancellationToken);

                    return null;
                }

                var result = new Messages.v1.Session
                {
                    IdentityId = content.IdentityId,
                    IdentityName = content.IdentityName,
                    DateRegistered = content.DateRegistered,
                    ExpiryDate = content.ExpiryDate,
                    Permissions = content.Permissions.ToList()
                };

                await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(result), cancellationToken);

                return Add(content.Token, result);
            }

            await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("IdentityName", identityName), cancellationToken);

            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            Flush();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task FlushAsync(Guid identityGuid, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            Flush();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask<bool> HasPermissionAsync(Guid identityId, string permission, CancellationToken cancellationToken = default)
    {
        var session = await FindAsync(identityId, cancellationToken);

        return session != null && HasPermission(session.IdentityId, permission);
    }

    public async Task AddAsync(Guid? token, Messages.v1.Session session, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (Find(session.IdentityId) != null)
            {
                return;
            }

            Add(token, session);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Messages.v1.Session?> FindByTokenAsync(Guid token, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var session = FindByToken(token);

            if (session != null)
            {
                await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(session), cancellationToken);

                return session;
            }

            var sessionResponse = await _accessClient.Sessions.PostSearchAsync(new()
            {
                Token = token,
                ShouldIncludePermissions = true
            }, cancellationToken);

            var tokenValue = token.ToString();

            if (sessionResponse is { IsSuccessStatusCode: true, Content: not null } && sessionResponse.Content.Any())
            {
                switch (sessionResponse.Content.Count())
                {
                    case 1:
                    {
                        var result = AddSession(token, sessionResponse.Content.Single());

                        await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(result), cancellationToken);

                        return result;
                    }
                    case > 1:
                    {
                        throw new InvalidOperationException(string.Format(Resources.UnexpectedMultipleSessionsException, "token", $"{tokenValue[..4]}****-****-****-****-********{tokenValue[^4..]}"));
                    }
                }
            }

            await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("Token", tokenValue), cancellationToken);

            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Messages.v1.Session?> FindAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var session = Find(identityId);

            if (session != null)
            {
                await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(session), cancellationToken);

                return session;
            }

            var sessionResponse = await _accessClient.Sessions.PostSearchAsync(new()
            {
                IdentityId = identityId,
                ShouldIncludePermissions = true
            }, cancellationToken);

            if (sessionResponse is { IsSuccessStatusCode: true, Content: not null } && sessionResponse.Content.Any())
            {
                switch (sessionResponse.Content.Count())
                {
                    case 1:
                    {
                        var result = AddSession(null, sessionResponse.Content.Single());

                        await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(result), cancellationToken);

                        return result;
                    }
                    case > 1:
                    {
                        throw new InvalidOperationException(string.Format(Resources.UnexpectedMultipleSessionsException, "IdentityId", identityId.ToString()));
                    }
                }
            }

            await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("IdentityId", identityId.ToString()), cancellationToken);

            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    private Messages.v1.Session AddSession(Guid? token, Messages.v1.Session session)
    {
        return Add(token, session);
    }
}