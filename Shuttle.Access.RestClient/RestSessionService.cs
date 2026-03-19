using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.WebApi.Contracts.v1;
using Shuttle.Core.Contract;
using Session = Shuttle.Access.Query.Session;

namespace Shuttle.Access.RestClient;

public class RestSessionService(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, ISessionCache sessionCache, IAccessClient accessClient, ILogger<RestSessionService>? logger = null)
    : ISessionService, IContextSessionService
{
    private readonly AccessAuthorizationOptions _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
    private readonly IAccessClient _accessClient = Guard.AgainstNull(accessClient);
    private readonly ILogger<RestSessionService> _logger = logger ?? NullLogger<RestSessionService>.Instance;
    private readonly ISessionCache _sessionCache = Guard.AgainstNull(sessionCache);

    public async Task<Session?> FindAsync(CancellationToken cancellationToken = default)
    {
        var sessionResponse = await _accessClient.Sessions.GetSelfAsync(cancellationToken);

        var result = sessionResponse is { IsSuccessStatusCode: true, Content: not null } ? _sessionCache.Add(GetSession(sessionResponse.Content)) : null;

        if (result == null)
        {
            LogMessage.SessionUnavailable(_logger, "Pass-Through", "(self)");

            await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("Pass-Through", "(self)"), cancellationToken);
        }
        else
        {
            LogMessage.SessionAvailable(_logger, result.IdentityName, result.TenantId);

            await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(result), cancellationToken);
        }

        return result;
    }

    public async Task<Session?> FindAsync(Session.Specification specification, CancellationToken cancellationToken = default)
    {
        var session = _sessionCache.Find(specification);

        if (session != null)
        {
            LogMessage.SessionAvailable(_logger, session.IdentityName, session.TenantId);

            await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(session), cancellationToken);

            return session;
        }

        if (_accessAuthorizationOptions.PassThrough)
        {
            session = await FindAsync(cancellationToken);

            if (session != null)
            {
                _sessionCache.Add(session);
            }

            return session;
        }

        var messageSpecification = new WebApi.Contracts.v1.Session.Specification
        {
            Id = specification.Id,
            IdentityId = specification.IdentityId,
            IdentityName = specification.IdentityName ?? string.Empty,
            IdentityNameMatch = specification.IdentityNameMatch ?? string.Empty,
            TenantId = specification.TenantId,
            TokenHash = specification.TokenHash
        };

        var sessionResponse = await _accessClient.Sessions.PostSearchAsync(messageSpecification, cancellationToken);

        if (sessionResponse is { IsSuccessStatusCode: true, Content: not null } && sessionResponse.Content.Any())
        {
            switch (sessionResponse.Content.Count())
            {
                case 1:
                {
                    var result = _sessionCache.Add(GetSession(sessionResponse.Content.Single()));

                    LogMessage.SessionAvailable(_logger, result.IdentityName, result.TenantId);

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

                var result = GetSession(content.Session);

                LogMessage.SessionAvailable(_logger, result.IdentityName, result.TenantId);

                await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(result), cancellationToken);

                return _sessionCache.Add(result);
            }
        }

        if (!string.IsNullOrWhiteSpace(specification.IdentityName))
        {
            LogMessage.SessionUnavailable(_logger, "IdentityName", specification.IdentityName);

            await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("IdentityName", specification.IdentityName), cancellationToken);
        }

        if (specification.TokenHash != null)
        {
            var identifier = Convert.ToHexString(specification.TokenHash);

            LogMessage.SessionUnavailable(_logger, "TokenHash", identifier);

            await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("IdentityName", identifier), cancellationToken);
        }

        return null;
    }

    private static Session GetSession(WebApi.Contracts.v1.Session session)
    {
        return new()
        {
            Id = session.Id,
            IdentityId = session.IdentityId,
            IdentityName = session.IdentityName,
            IdentityDescription = session.IdentityDescription,
            DateRegistered = session.DateRegistered,
            ExpiryDate = session.ExpiryDate,
            TenantId = session.TenantId,
            TenantName = session.TenantName,
            Permissions = session.Permissions.Select(e => new Query.Permission
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Status = (PermissionStatus)e.Status
            }).ToList()
        };
    }
}