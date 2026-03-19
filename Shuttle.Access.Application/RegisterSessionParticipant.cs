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
                var requesterSession = (await sessionQuery.SearchAsync(new Session.Specification().WithTokenHash(hashingService.Sha256(message.GetAuthenticationToken().ToString("D"))), cancellationToken)).FirstOrDefault();

                if (requesterSession == null || DateTimeOffset.UtcNow > requesterSession.ExpiryDate || !requesterSession.HasPermission(AccessPermissions.Sessions.Register))
                {
                    message.DelegationSessionInvalid();

                    return;
                }

                break;
            }
            case SessionRegistrationType.Token:
            case SessionRegistrationType.Direct:
            {
                break;
            }
            default:
            {
                throw new InvalidOperationException(string.Format(Resources.SessionRegistrationTypeNoneException, message.RegistrationType));
            }
        }

        var identity = (await identityQuery.SearchAsync(new Query.Identity.Specification().WithName(message.IdentityName), cancellationToken)).SingleOrDefault();

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

        if (message.TenantId.HasValue && tenants.All(item => item.Id != message.TenantId.Value))
        {
            message.Forbidden();
            return;
        }

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

        if (!message.TenantId.HasValue)
        {
            message.Forbidden();
            return;
        }

        message.WithTenants(tenants);

        var session = message.RegistrationType == SessionRegistrationType.Token
            ? (await sessionQuery.SearchAsync(new Session.Specification().WithTokenHash(hashingService.Sha256(message.GetAuthenticationToken().ToString("D"))), cancellationToken)).FirstOrDefault()
            : (await sessionQuery.SearchAsync(new Session.Specification().WithTenantId(message.TenantId.Value).WithIdentityName(message.IdentityName), cancellationToken)).FirstOrDefault();

        if (session != null)
        {
            if (DateTimeOffset.UtcNow <= session.ExpiryDate)
            {
                var token = message.RegistrationType == SessionRegistrationType.Token
                    ? message.GetAuthenticationToken()
                    : Guid.NewGuid();

                await SaveAsync(token);

                return;
            }

            if (session.ExpiryDate.Add(accessOptions.Value.SessionRenewalTolerance) > DateTimeOffset.UtcNow)
            {
                await SaveAsync(Guid.NewGuid());

                return;
            }
        }

        if (message.RegistrationType != SessionRegistrationType.Token)
        {
            var now = DateTimeOffset.UtcNow;
            var token = Guid.NewGuid();

            session = new()
            {
                Id = session?.Id ?? Guid.NewGuid(),
                TenantId = message.TenantId.Value,
                IdentityId = identity.Id,
                DateRegistered = now
            };

            await SaveAsync(token);
        }

        return;

        async Task SaveAsync(Guid token)
        {
            session.TokenHash = hashingService.Sha256(token.ToString("D"));
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

            message.Registered(token, session);
        }
    }
}