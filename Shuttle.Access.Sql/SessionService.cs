using System;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Sql
{
    public class SessionService : ISessionService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAccessConfiguration _configuration;
        private readonly ISessionRepository _sessionRepository;

        public SessionService(IAccessConfiguration configuration, IAuthenticationService authenticationService,
            IAuthorizationService authorizationService, ISessionRepository sessionRepository)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(authenticationService, nameof(authenticationService));
            Guard.AgainstNull(authorizationService, nameof(authorizationService));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));

            _configuration = configuration;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _sessionRepository = sessionRepository;
        }

        public RegisterSessionResult Register(string username, string password, Guid token)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) && token.Equals(Guid.Empty))
            {
                return RegisterSessionResult.Failure();
            }

            Session session;

            if (!string.IsNullOrEmpty(password))
            {
                var authenticationResult = _authenticationService.Authenticate(username, password);

                if (!authenticationResult.Authenticated)
                {
                    return RegisterSessionResult.Failure();
                }

                var now = DateTime.Now;

                session = new Session(Guid.NewGuid(), username, now, now.Add(_configuration.SessionDuration));

                foreach (var permission in _authorizationService.Permissions(username,
                    authenticationResult.AuthenticationTag))
                {
                    session.AddPermission(permission);
                }

                _sessionRepository.Save(session);
            }
            else
            {
                session = _sessionRepository.Find(token);

                if (session == null)
                {
                    return RegisterSessionResult.Failure();
                }

                if (session.HasExpired && session.ExpiryDate.Subtract(_configuration.SessionDuration) < DateTime.Now)
                {
                    session.Renew(DateTime.Now.Add(_configuration.SessionDuration));

                    _sessionRepository.Save(session);
                }
                else
                {
                    return RegisterSessionResult.Failure();
                }
            }

            return RegisterSessionResult.Success(session.Username, session.Token, session.Permissions);
        }

        public RegisterSessionResult Register(string username, Guid token)
        {
            Guard.AgainstNullOrEmptyString(username, nameof(username));

            var authenticationResult = _authenticationService.Authenticate(username);

            if (!authenticationResult.Authenticated)
            {
                return RegisterSessionResult.Failure();
            }

            var now = DateTime.Now;

            var session = new Session(token, username, now, now.Add(_configuration.SessionDuration));

            foreach (var permission in _authorizationService.Permissions(username,
                authenticationResult.AuthenticationTag))
            {
                session.AddPermission(permission);
            }

            _sessionRepository.Save(session);

            return RegisterSessionResult.Success(session.Username, session.Token, session.Permissions);
        }

        public bool Remove(Guid token)
        {
            var session = _sessionRepository.Find(token);

            return session != null && Remove(session.Username);
        }

        public bool Remove(string username)
        {
            Guard.AgainstNullOrEmptyString(username, nameof(username));

            return _sessionRepository.Remove(username) > 0;
        }
    }
}