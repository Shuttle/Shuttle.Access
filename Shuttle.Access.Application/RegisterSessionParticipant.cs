using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class RegisterSessionParticipant : IAsyncParticipant<RegisterSession>
{
    private readonly AccessOptions _accessOptions;
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IIdentityQuery _identityQuery;
    private readonly ISessionRepository _sessionRepository;

    public RegisterSessionParticipant(IOptions<AccessOptions> accessOptions, IAuthenticationService authenticationService, IAuthorizationService authorizationService, ISessionRepository sessionRepository, IIdentityQuery identityQuery)
    {
        _accessOptions = Guard.AgainstNull(accessOptions).Value;
        _authenticationService = authenticationService;
        _authorizationService = Guard.AgainstNull(authorizationService);
        _sessionRepository = Guard.AgainstNull(sessionRepository);
        _identityQuery = Guard.AgainstNull(identityQuery);
    }

    public async Task ProcessMessageAsync(IParticipantContext<RegisterSession> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message;

        switch (message.RegistrationType)
        {
            case SessionRegistrationType.Password:
            {
                var authenticationResult = await _authenticationService.AuthenticateAsync(message.IdentityName, message.GetPassword(), context.CancellationToken);

                if (!authenticationResult.Authenticated)
                {
                    message.Forbidden();
                    return;
                }

                break;
            }
            case SessionRegistrationType.Delegation:
            {
                var requesterSession = await _sessionRepository.FindAsync(message.GetToken(), context.CancellationToken);

                if (requesterSession == null || requesterSession.HasExpired || !requesterSession.HasPermission(Permissions.Register.Session))
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

        if ((await _identityQuery.SearchAsync(new DataAccess.Query.Identity.Specification().WithName(message.IdentityName), context.CancellationToken)).SingleOrDefault() == null)
        {
            message.UnknownIdentity();

            return;
        }

        var session = message.RegistrationType == SessionRegistrationType.Token
            ? await _sessionRepository.FindAsync(message.GetToken(), context.CancellationToken)
            : await _sessionRepository.FindAsync(message.IdentityName, context.CancellationToken);

        if (session != null)
        {
            if (!session.HasExpired)
            {
                message.Registered(session);

                return;
            }

            if (session.ExpiryDate.Add(_accessOptions.SessionRenewalTolerance) > DateTime.UtcNow)
            {
                session.Renew(DateTime.UtcNow.Add(_accessOptions.SessionDuration));

                await _sessionRepository.RenewAsync(session, context.CancellationToken);

                message.Registered(session);

                return;
            }
        }

        if (message.RegistrationType != SessionRegistrationType.Token)
        {
            var now = DateTime.UtcNow;

            session = new(Guid.NewGuid(), await _identityQuery.IdAsync(message.IdentityName, context.CancellationToken), message.IdentityName, now, now.Add(_accessOptions.SessionDuration));

            foreach (var permission in await _authorizationService.GetPermissionsAsync(message.IdentityName, context.CancellationToken))
            {
                session.AddPermission(permission);
            }

            await _sessionRepository.SaveAsync(session, context.CancellationToken);

            message.Registered(session);
        }
    }
}