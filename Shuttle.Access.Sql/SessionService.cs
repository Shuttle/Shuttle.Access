using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Sql
{
    public class SessionService : ISessionService
    {
        private readonly AccessOptions _accessOptions;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISessionRepository _sessionRepository;
        private readonly IIdentityQuery _identityQuery;

        public SessionService(IOptions<AccessOptions> accessOptions, IAuthenticationService authenticationService,  IAuthorizationService authorizationService, ISessionRepository sessionRepository,  IIdentityQuery identityQuery)
        {
            Guard.AgainstNull(accessOptions, nameof(accessOptions));
            Guard.AgainstNull(accessOptions.Value, nameof(accessOptions.Value));
            Guard.AgainstNull(authenticationService, nameof(authenticationService));
            Guard.AgainstNull(authorizationService, nameof(authorizationService));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));

            _accessOptions = accessOptions.Value;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _sessionRepository = sessionRepository;
            _identityQuery = identityQuery;
        }

        public RegisterSessionResult Register(string identityName, Guid requesterToken)
        {
            if (string.IsNullOrEmpty(identityName))
            {
                return RegisterSessionResult.Failure();
            }

            var requesterSession = _sessionRepository.Find(requesterToken);

            if (requesterSession == null)
            {
                SessionOperation.Invoke(this, new SessionOperationEventArgs(string.Format(Access.Resources.SessionRegisterRequestInvalidToken, requesterToken)));

                return RegisterSessionResult.Failure();
            }

            if (requesterSession.HasExpired)
            {
                SessionOperation.Invoke(this, new SessionOperationEventArgs(string.Format(Access.Resources.SessionRegisterRequestExpired, requesterSession.IdentityName)));

                return RegisterSessionResult.Failure();
            }

            if (!requesterSession.HasPermission("access://identity/register-session"))
            {
                SessionOperation.Invoke(this, new SessionOperationEventArgs(string.Format(Access.Resources.SessionRegisterRequestDenied, requesterSession.IdentityName, "access://identity/register-session")));

                return RegisterSessionResult.Failure();
            }

            if (_identityQuery.Search(new DataAccess.Query.Identity.Specification().WithName(identityName))
                .SingleOrDefault() == null)
            {
                return RegisterSessionResult.Failure();
            }
            
            var session = _sessionRepository.Find(identityName);

            if (session != null && 
                session.HasExpired &&
                session.ExpiryDate.Subtract(_accessOptions.SessionDuration) < DateTime.UtcNow)
            {
                    SessionOperation.Invoke(this, new SessionOperationEventArgs(string.Format(Access.Resources.SessionRegisterRenewed, identityName)));

                    session.Renew(DateTime.UtcNow.Add(_accessOptions.SessionDuration));

                    _sessionRepository.Renew(session);
            }
            else
            {
                var now = DateTime.UtcNow;

                session = new Session(Guid.NewGuid(), _identityQuery.Id(identityName), identityName, now,
                    now.Add(_accessOptions.SessionDuration));

                foreach (var permission in _authorizationService.Permissions(identityName))
                {
                    session.AddPermission(permission);
                }

                _sessionRepository.Save(session);
            }

            return RegisterSessionResult.Success(session);
        }

        public RegisterSessionResult Register(string identityName, string password, Guid token)
        {
            if (string.IsNullOrEmpty(identityName) || string.IsNullOrEmpty(password) && token.Equals(Guid.Empty))
            {
                return RegisterSessionResult.Failure();
            }

            Session session;

            if (!string.IsNullOrEmpty(password))
            {
                SessionOperation.Invoke(this, new SessionOperationEventArgs(string.Format(Access.Resources.SessionRegisterIdentity, identityName)));

                var authenticationResult = _authenticationService.Authenticate(identityName, password);

                if (!authenticationResult.Authenticated)
                {
                    SessionOperation.Invoke(this, new SessionOperationEventArgs(string.Format(Access.Resources.SessionRegisterFailure, identityName)));

                    return RegisterSessionResult.Failure();
                }

                session = _sessionRepository.Find(identityName);

                if (session != null && session.HasExpired)
                {
                    if (session.ExpiryDate.Subtract(_accessOptions.SessionDuration) < DateTime.UtcNow)
                    {
                        SessionOperation.Invoke(this, new SessionOperationEventArgs(string.Format(Access.Resources.SessionRegisterRenewed, identityName)));

                        session.Renew(DateTime.UtcNow.Add(_accessOptions.SessionDuration));

                        _sessionRepository.Renew(session);
                    }
                    else
                    {
                        session = null;
                    }
                }

                if (session == null)
                {
                    var now = DateTime.UtcNow;

                    session = new Session(Guid.NewGuid(), _identityQuery.Id(identityName), identityName, now,
                        now.Add(_accessOptions.SessionDuration));

                    foreach (var permission in _authorizationService.Permissions(identityName))
                    {
                        session.AddPermission(permission);
                    }

                    _sessionRepository.Save(session);
                }
            }
            else
            {
                SessionOperation.Invoke(this, new SessionOperationEventArgs(string.Format(Access.Resources.SessionRegisterToken, identityName)));

                session = _sessionRepository.Find(token);

                if (session == null)
                {
                    SessionOperation.Invoke(this, new SessionOperationEventArgs(string.Format(Access.Resources.SessionRegisterInvalidToken, identityName)));

                    return RegisterSessionResult.Failure();
                }

                if (session.HasExpired)
                {
                    if (session.ExpiryDate.Subtract(_accessOptions.SessionDuration) < DateTime.UtcNow)
                    {
                        SessionOperation.Invoke(this, new SessionOperationEventArgs(string.Format(Access.Resources.SessionRegisterRenewed, identityName)));

                        session.Renew(DateTime.UtcNow.Add(_accessOptions.SessionDuration));

                        _sessionRepository.Renew(session);
                    }
                    else
                    {
                        SessionOperation.Invoke(this, new SessionOperationEventArgs(string.Format(Access.Resources.SessionRegisterExpired, identityName)));

                        return RegisterSessionResult.Failure();
                    }
                }
            }

            SessionOperation.Invoke(this, new SessionOperationEventArgs(string.Format(Access.Resources.SessionRegisterSuccess, identityName)));

            return RegisterSessionResult.Success(session.IdentityName, session.Token, session.ExpiryDate, session.Permissions);
        }

        public bool Remove(Guid token)
        {
            var session = _sessionRepository.Find(token);

            return session != null && Remove(session.IdentityName);
        }

        public bool Remove(string identityName)
        {
            Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));

            return _sessionRepository.Remove(identityName) > 0;
        }

        public void Refresh(Guid token)
        {
            var session = _sessionRepository.Get(token);

            session.ClearPermissions();

            foreach (var permission in _authorizationService.Permissions(session.IdentityName))
            {
                session.AddPermission(permission);
            }

            _sessionRepository.Save(session);
        }

        public event EventHandler<SessionOperationEventArgs> SessionOperation = delegate
        {
        };
    }
}