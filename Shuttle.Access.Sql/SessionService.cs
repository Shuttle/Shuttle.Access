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
        private readonly IIdentityQuery _userQuery;

        public SessionService(IAccessConfiguration configuration, IAuthenticationService authenticationService,
            IAuthorizationService authorizationService, ISessionRepository sessionRepository,
            IIdentityQuery userQuery)
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

        public RegisterSessionResult Register(string identityName, string password, Guid token)
        {
            if (string.IsNullOrEmpty(identityName) || string.IsNullOrEmpty(password) && token.Equals(Guid.Empty))
            {
                return RegisterSessionResult.Failure();
            }

            Session session;

            if (!string.IsNullOrEmpty(password))
            {
                var authenticationResult = _authenticationService.Authenticate(identityName, password);

                if (!authenticationResult.Authenticated)
                {
                    return RegisterSessionResult.Failure();
                }

                var now = DateTime.Now;

                session = new Session(Guid.NewGuid(), _userQuery.Id(identityName), identityName, now, now.Add(_configuration.SessionDuration));

                foreach (var permission in _authorizationService.Permissions(identityName,
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

            return RegisterSessionResult.Success(session.IdentityName, session.Token, session.Permissions);
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