using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class RestSessionService(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, ISessionCache sessionCache, IAccessClient accessClient, ILogger<RestSessionService>? logger = null)
    : ISessionService, IContextSessionService
{
    private readonly ILogger<RestSessionService> _logger = logger ?? NullLogger<RestSessionService>.Instance;
    private readonly AccessAuthorizationOptions _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
    private readonly IAccessClient _accessClient = Guard.AgainstNull(accessClient);
    private readonly ISessionCache _sessionCache = Guard.AgainstNull(sessionCache);

    public async Task<Messages.v1.Session?> FindAsync(CancellationToken cancellationToken = default)
    {
        var sessionResponse = await _accessClient.Sessions.GetSelfAsync(cancellationToken);

        var result = sessionResponse is { IsSuccessStatusCode: true, Content: not null } ? _sessionCache.Add(null, sessionResponse.Content) : null;

        if (result == null)
        {
            LogMessage.SessionUnavailable(_logger, "Pass-Through", "(self)");

            await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("Pass-Through", "(self)"), cancellationToken);
        }
        else
        {
            LogMessage.SessionAvailable(_logger, result.IdentityName, string.IsNullOrWhiteSpace(result.TenantName) ? "(selection-required)" : result.TenantName);

            await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(result), cancellationToken);
        }

        return result;
    }

    public async Task<Messages.v1.Session?> FindAsync(Messages.v1.Session.Specification specification, CancellationToken cancellationToken = default)
    {
        var session = _sessionCache.Find(specification);

        if (session != null)
        {
            LogMessage.SessionAvailable(_logger, session.IdentityName, string.IsNullOrWhiteSpace(session.TenantName) ? "(selection-required)" : session.TenantName);
            
            await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(session), cancellationToken);

            return session;
        }

        if (_accessAuthorizationOptions.PassThrough)
        {
            session = await FindAsync(cancellationToken);

            if (session != null)
            {
                _sessionCache.Add(specification.Token, session);
            }

            return session;
        }

        specification.ShouldIncludePermissions = true;

        var sessionResponse = await _accessClient.Sessions.PostSearchAsync(specification, cancellationToken);

        if (sessionResponse is { IsSuccessStatusCode: true, Content: not null } && sessionResponse.Content.Any())
        {
            switch (sessionResponse.Content.Count())
            {
                case 1:
                {
                    var result = _sessionCache.Add(specification.Token, sessionResponse.Content.Single());

                    LogMessage.SessionAvailable(_logger, result.IdentityName, string.IsNullOrWhiteSpace(result.TenantName) ? "(selection-required)" : result.TenantName);

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
                    LogMessage.SessionUnavailable(_logger, "IdentityName", specification.IdentityName);

                    await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("IdentityName", specification.IdentityName), cancellationToken);

                    return null;
                }

                var result = new Messages.v1.Session
                {
                    TenantId = content.TenantId,
                    TenantName =  content.TenantName,
                    IdentityId = content.IdentityId,
                    IdentityName = content.IdentityName,
                    DateRegistered = content.DateRegistered,
                    ExpiryDate = content.ExpiryDate,
                    Permissions = content.Permissions.ToList()
                };

                LogMessage.SessionAvailable(_logger, result.IdentityName, string.IsNullOrWhiteSpace(result.TenantName) ? "(selection-required)" : result.TenantName);

                await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(result), cancellationToken);

                return _sessionCache.Add(content.Token, result);
            }
        }

        LogMessage.SessionUnavailable(_logger, "IdentityName", specification.IdentityName);

        await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("IdentityName", specification.IdentityName), cancellationToken);

        return null;
    }
}