using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class RegisterSessionParticipant(IOptions<AccessOptions> accessOptions, IAuthenticationService authenticationService, IHashingService hashingService, ISessionRepository sessionRepository, ISessionQuery sessionQuery, IIdentityQuery identityQuery)
    : IParticipant<RegisterSession>
{
    public async Task HandleAsync(RegisterSession message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);
        Guard.AgainstNull(accessOptions);
        Guard.AgainstNull(authenticationService);
        Guard.AgainstNull(hashingService);
        Guard.AgainstNull(sessionRepository);
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
                var requesterSession = await sessionRepository.FindAsync(new Query.Session.Specification().WithTokenHash(hashingService.Sha256(message.GetAuthenticationToken().ToString("D"))), cancellationToken);

                if (requesterSession == null || requesterSession.HasExpired || !requesterSession.HasPermission(AccessPermissions.Sessions.Register))
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
            ? await sessionRepository.FindAsync(new Query.Session.Specification().WithTokenHash(hashingService.Sha256(message.GetAuthenticationToken().ToString("D"))), cancellationToken) 
            : await sessionRepository.FindAsync(new Query.Session.Specification().WithTenantId(message.TenantId.Value).WithIdentityName(message.IdentityName), cancellationToken);

        if (session != null)
        {
            if (!session.HasExpired)
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

            session = new( Guid.NewGuid(), hashingService.Sha256(token.ToString("D")), message.TenantId.Value, identity.Id, now, now.Add(accessOptions.Value.SessionDuration));

            await SaveAsync(token);
        }

        return;

        async Task SaveAsync(Guid token)
        {
            foreach (var permission in await identityQuery.PermissionsAsync(identity.Id, message.TenantId.Value, cancellationToken))
            {
                session.AddPermission(new(permission.Id, permission.Name, permission.Description, permission.Status));
            }

            session.Renew(DateTimeOffset.UtcNow.Add(accessOptions.Value.SessionDuration), hashingService.Sha256(token.ToString("D")));

            await sessionRepository.SaveAsync(session, cancellationToken);

            message.Registered(token, (await sessionQuery.SearchAsync(new Query.Session.Specification().WithId(session.Id), cancellationToken)).First());
        }
    }
}