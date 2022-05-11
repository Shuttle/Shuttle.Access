using System;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;

namespace Shuttle.Access.Sql
{
    public class SessionService : ISessionService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAccessConfiguration _configuration;
        private readonly ISessionRepository _sessionRepository;
        private readonly IIdentityQuery _identityQuery;
        private readonly ILog _log;

        public SessionService(IAccessConfiguration configuration, IAuthenticationService authenticationService,
            IAuthorizationService authorizationService, ISessionRepository sessionRepository,
            IIdentityQuery identityQuery)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(authenticationService, nameof(authenticationService));
            Guard.AgainstNull(authorizationService, nameof(authorizationService));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));

            _configuration = configuration;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _sessionRepository = sessionRepository;
            _identityQuery = identityQuery;

            _log = Log.For(this);
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
                _log.Debug(string.Format(Resources.SessionRegisterRequestInvalidToken, requesterToken));

                return RegisterSessionResult.Failure();
            }

            if (requesterSession.HasExpired)
            {
                _log.Debug(string.Format(Resources.SessionRegisterRequestExpired, requesterSession.IdentityName));

                return RegisterSessionResult.Failure();
            }

            if (!requesterSession.HasPermission("access://identity/register-session"))
            {
                _log.Debug(string.Format(Resources.SessionRegisterRequestDenied, requesterSession.IdentityName, "access://identity/register-session"));

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
                session.ExpiryDate.Subtract(_configuration.SessionDuration) < DateTime.UtcNow)
            {
                    _log.Debug(string.Format(Resources.SessionRegisterRenewed, identityName));

                    session.Renew(DateTime.UtcNow.Add(_configuration.SessionDuration));

                    _sessionRepository.Renew(session);
            }
            else
            {
                var now = DateTime.UtcNow;

                session = new Session(Guid.NewGuid(), _identityQuery.Id(identityName), identityName, now,
                    now.Add(_configuration.SessionDuration));

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
                _log.Debug(string.Format(Resources.SessionRegisterIdentity, identityName));

                var authenticationResult = _authenticationService.Authenticate(identityName, password);

                if (!authenticationResult.Authenticated)
                {
                    _log.Debug(string.Format(Resources.SessionRegisterFailure, identityName));

                    return RegisterSessionResult.Failure();
                }

                var now = DateTime.UtcNow;

                session = new Session(Guid.NewGuid(), _identityQuery.Id(identityName), identityName, now, now.Add(_configuration.SessionDuration));

                foreach (var permission in _authorizationService.Permissions(identityName))
                {
                    session.AddPermission(permission);
                }

                _sessionRepository.Save(session);
            }
            else
            {
                _log.Debug(string.Format(Resources.SessionRegisterToken, identityName));

                session = _sessionRepository.Find(token);

                if (session == null)
                {
                    _log.Debug(string.Format(Resources.SessionRegisterInvalidToken, identityName));

                    return RegisterSessionResult.Failure();
                }

                if (session.HasExpired)
                {
                    if (session.ExpiryDate.Subtract(_configuration.SessionDuration) < DateTime.UtcNow)
                    {
                        _log.Debug(string.Format(Resources.SessionRegisterRenewed, identityName));

                        session.Renew(DateTime.UtcNow.Add(_configuration.SessionDuration));

                        _sessionRepository.Renew(session);
                    }
                    else
                    {
                        _log.Debug(string.Format(Resources.SessionRegisterExpired, identityName));

                        return RegisterSessionResult.Failure();
                    }
                }
            }

            _log.Debug(string.Format(Resources.SessionRegisterSuccess, identityName));

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
    }
}