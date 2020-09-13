using System;
using System.Diagnostics;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Sql
{
    public class SessionService : ISessionService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAccessConfiguration _configuration;
        private readonly ISessionRepository _sessionRepository;
        private readonly ISystemUserQuery _userQuery;

        public SessionService(IAccessConfiguration configuration, IAuthenticationService authenticationService,
            IAuthorizationService authorizationService, ISessionRepository sessionRepository,
            ISystemUserQuery userQuery)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(authenticationService, nameof(authenticationService));
            Guard.AgainstNull(authorizationService, nameof(authorizationService));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));
            Guard.AgainstNull(userQuery, nameof(userQuery));

            _configuration = configuration;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _sessionRepository = sessionRepository;
            _userQuery = userQuery;
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

                session = new Session(Guid.NewGuid(), _userQuery.Id(username), username, now, now.Add(_configuration.SessionDuration));

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

                if (session.HasExpired)
                {
                    if (session.ExpiryDate.Subtract(_configuration.SessionDuration) < DateTime.Now)
                    {
                        session.Renew(DateTime.Now.Add(_configuration.SessionDuration));

                        _sessionRepository.Renew(session);
                    }
                    else
                    {
                        return RegisterSessionResult.Failure();
                    }
                }
            }

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