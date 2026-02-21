using Microsoft.Extensions.Options;
using Shuttle.Access.Query;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class RegisterSessionParticipant(IOptions<AccessOptions> accessOptions, IAuthenticationService authenticationService, IAuthorizationService authorizationService, IHashingService hashingService, ISessionRepository sessionRepository, IIdentityQuery identityQuery)
    : IParticipant<RegisterSession>
{
    private readonly AccessOptions _accessOptions = Guard.AgainstNull(accessOptions).Value;
    private readonly IAuthenticationService _authenticationService = Guard.AgainstNull(authenticationService);
    private readonly IAuthorizationService _authorizationService = Guard.AgainstNull(authorizationService);
    private readonly IHashingService _hashingService = Guard.AgainstNull(hashingService);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly ISessionRepository _sessionRepository = Guard.AgainstNull(sessionRepository);

    public async Task HandleAsync(RegisterSession message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        switch (message.RegistrationType)
        {
            case SessionRegistrationType.Password:
            {
                var authenticationResult = await _authenticationService.AuthenticateAsync(message.IdentityName, message.GetPassword(), cancellationToken);

                if (!authenticationResult.Authenticated)
                {
                    message.Forbidden();
                    return;
                }

                break;
            }
            case SessionRegistrationType.Delegation:
            {
                var requesterSession = await _sessionRepository.FindAsync(new SessionSpecification().WithToken(_hashingService.Sha256(message.GetAuthenticationToken().ToString("D"))), cancellationToken);

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

        var identity = (await _identityQuery.SearchAsync(new IdentitySpecification().WithName(message.IdentityName), cancellationToken)).SingleOrDefault();

        if (identity == null)
        {
            message.UnknownIdentity();
            return;
        }

        if (identity.IdentityTenants.Count == 0)
        {
            message.Forbidden();
            return;
        }

        message.WithIdentity(identity);
        
        Session? session = null;

        if (message.RegistrationType == SessionRegistrationType.Token)
        {
            session = await _sessionRepository.FindAsync(new SessionSpecification().WithToken(_hashingService.Sha256(message.GetAuthenticationToken().ToString("D"))), cancellationToken);
        }
        else
        {
            if (message.TenantId.HasValue)
            {
                session = await _sessionRepository.FindAsync(new SessionSpecification().WithTenantId(message.TenantId.Value).WithIdentityName(message.IdentityName), cancellationToken);
            }
        }

        if (session != null)
        {
            if (!session.HasExpired)
            {
                var token = message.RegistrationType == SessionRegistrationType.Token
                    ? message.GetAuthenticationToken()
                    : Guid.NewGuid();

                await SaveAsync(token);

                message.Registered(token, session);

                return;
            }

            if (session.ExpiryDate.Add(_accessOptions.SessionRenewalTolerance) > DateTimeOffset.UtcNow)
            {
                var token = Guid.NewGuid();

                await SaveAsync(token);

                message.Registered(token, session);

                return;
            }
        }

        if (message.RegistrationType != SessionRegistrationType.Token)
        {
            var now = DateTimeOffset.UtcNow;
            var token = Guid.NewGuid();

            session = new( Guid.NewGuid(), _hashingService.Sha256(token.ToString("D")), await _identityQuery.IdAsync(message.IdentityName, cancellationToken), message.IdentityName, now, now.Add(_accessOptions.SessionDuration));

            message.Registered(token, session);

            await SaveAsync(token);
        }

        return;

        async Task SaveAsync(Guid token)
        {
            if (message.TenantId.HasValue)
            {
                foreach (var permission in await _authorizationService.GetPermissionsAsync(message.IdentityName, message.TenantId.Value, cancellationToken))
                {
                    session.AddPermission(new(permission.Id, permission.Name));
                }
            }

            session.Renew(DateTimeOffset.UtcNow.Add(_accessOptions.SessionDuration), _hashingService.Sha256(token.ToString("D")));

            await _sessionRepository.SaveAsync(session, cancellationToken);
        }
    }
}