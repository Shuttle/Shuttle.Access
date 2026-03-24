using Microsoft.Extensions.Options;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class RegisterSessionParticipant(IOptions<AccessOptions> accessOptions, IAuthenticationService authenticationService, IHashingService hashingService, ISessionQuery sessionQuery, IIdentityQuery identityQuery)
    : IParticipant<RegisterSession>
{
    public async Task HandleAsync(RegisterSession message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);
        Guard.AgainstNull(accessOptions);
        Guard.AgainstNull(authenticationService);
        Guard.AgainstNull(hashingService);
        Guard.AgainstNull(sessionQuery);
        Guard.AgainstNull(identityQuery);

        Session? session;

        if (message.ShouldRefresh)
        {
            var specification = new Session.Specification().WithIdentityName(message.IdentityName);

            var sessions = await sessionQuery.SearchAsync(specification, cancellationToken);

            if (sessions.Any())
            {
                await sessionQuery.RemoveAsync(specification, cancellationToken);
                message.WithSessionsRemoved(sessions);
            }
        }

        switch (message.RegistrationType)
        {
            case SessionRegistrationType.Password:
            {
                var authenticationResult = await authenticationService.AuthenticateAsync(message.IdentityName, message.GetPassword(), cancellationToken);

                if (!authenticationResult.Authenticated)
                {
                    message.Forbidden();
                    return;
                }

                break;
            }
            case SessionRegistrationType.Delegation:
            {
                var requesterSession = (await sessionQuery.SearchAsync(new Session.Specification().WithTokenHash(hashingService.Sha256(message.GetSessionToken().ToString("D"))), cancellationToken)).FirstOrDefault();

                if (requesterSession == null || DateTimeOffset.UtcNow > requesterSession.ExpiryDate || !requesterSession.HasPermission(AccessPermissions.Sessions.Register))
                {
                    message.DelegationSessionInvalid();
                    return;
                }

                break;
            }
            case SessionRegistrationType.Token:
            {
                session = (await sessionQuery.SearchAsync(new Session.Specification().WithTokenHash(hashingService.Sha256(message.GetSessionToken().ToString("D"))), cancellationToken)).FirstOrDefault();

                if (session != null)
                {
                    message.WithTenantId(session.TenantId);
                    await SaveAsync(message, session, cancellationToken);
                }
                else
                {
                    message.Forbidden();
                    return;
                }

                break;
            }
            case SessionRegistrationType.Direct:
            {
                break;
            }
            default:
            {
                throw new InvalidOperationException(string.Format(Resources.SessionRegistrationTypeNoneException, message.RegistrationType));
            }
        }

        var identity = (await identityQuery.SearchAsync(new Query.Identity.Specification().WithName(message.IdentityName).IncludeTenants(), cancellationToken)).SingleOrDefault();

        if (identity == null)
        {
            message.UnknownIdentity();
            return;
        }

        var tenants = identity.Tenants
            .Where(item => item.Status == TenantStatus.Active)
            .Select(item => new Query.Tenant
            {
                Id = item.Id,
                Name = item.Name,
                LogoSvg = item.LogoSvg,
                LogoUrl = item.LogoUrl
            })
            .ToList();

        message.WithTenants(tenants);

        if (message.TenantId.HasValue)
        {
            if (tenants.All(item => item.Id != message.TenantId.Value))
            {
                message.Forbidden();
                return;
            }
        }
        else
        {
            switch (tenants.Count)
            {
                case 0:
                {
                    message.Forbidden();
                    return;
                }
                case 1:
                {
                    message.WithTenantId(tenants[0].Id);
                    break;
                }
                case > 1:
                {
                    message.WithTenantId(tenants.Any(item => item.Id == accessOptions.Value.SystemTenantId)
                        ? accessOptions.Value.SystemTenantId
                        : tenants.First().Id);
                    break;
                }
            }
        }

        if (!message.TenantId.HasValue)
        {
            message.Forbidden();
            return;
        }

        if (message.RegistrationType == SessionRegistrationType.Token)
        {
            return;
        }

        session = (await sessionQuery.SearchAsync(new Session.Specification().WithTenantId(message.TenantId.Value).WithIdentityName(message.IdentityName), cancellationToken)).FirstOrDefault();

        if (session != null)
        {
            await SaveAsync(message, session, cancellationToken);

            return;
        }

        var now = DateTimeOffset.UtcNow;

        session = new()
        {
            Id = session?.Id ?? Guid.NewGuid(),
            TenantId = message.TenantId.Value,
            TenantName = tenants.FirstOrDefault(item => item.Id == message.TenantId.Value)?.Name ?? string.Empty,
            IdentityId = identity.Id,
            IdentityName = identity.Name,
            IdentityDescription = identity.Description,
            DateRegistered = now,
        };

        await SaveAsync(message, session, cancellationToken);
    }
    private async Task SaveAsync(RegisterSession message, Session session, CancellationToken cancellationToken)
    {
        Guid? token = session.ExpiryDate.Add(accessOptions.Value.SessionRenewalTolerance) > DateTimeOffset.UtcNow ? null : Guid.NewGuid();

        if (token.HasValue)
        {
            session.TokenHash = hashingService.Sha256(token.Value.ToString("D"));
        }

        session.ExpiryDate = DateTime.UtcNow.Add(accessOptions.Value.SessionDuration);
        session.Permissions = (await identityQuery.PermissionsAsync(session.IdentityId, session.TenantId, cancellationToken))
            .Select(permission => new Query.Permission
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description,
                Status = permission.Status
            })
            .ToList();

        await sessionQuery.SaveAsync(session, cancellationToken);

        if (token.HasValue)
        {
            message.Registered(token.Value, session);
        }
        else
        {
            message.Renewed(session);
        }
    }
}