using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SessionService : ISessionService
    {
        private readonly IAccessConfiguration _configuration;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISessionRepository _sessionRepository;

        public SessionService(IAccessConfiguration configuration, IDatabaseContextFactory databaseContextFactory,
            IAuthenticationService authenticationService, IAuthorizationService authorizationService,
            ISessionRepository sessionRepository)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(authenticationService, nameof(authenticationService));
            Guard.AgainstNull(authorizationService, nameof(authorizationService));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));

            _configuration = configuration;
            _databaseContextFactory = databaseContextFactory;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _sessionRepository = sessionRepository;
        }

        public RegisterSessionResult Register(string username, string password, Guid token)
        {
            if (string.IsNullOrEmpty(username) || (string.IsNullOrEmpty(password) && token.Equals(Guid.Empty)))
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

                session = new Session(Guid.NewGuid(), username, DateTime.Now);

                foreach (var permission in _authorizationService.Permissions(username, authenticationResult.AuthenticationTag))
                {
                    session.AddPermission(permission);
                }

                using (_databaseContextFactory.Create(_configuration.ProviderName, _configuration.ConnectionString))
                {
                    _sessionRepository.Save(session);
                }
            }
            else
            {
                using (_databaseContextFactory.Create(_configuration.ProviderName, _configuration.ConnectionString))
                {
                    session = _sessionRepository.Get(token);

                    if (session == null)
                    {
                        return RegisterSessionResult.Failure();
                    }

                    session.Renew();

                    _sessionRepository.Renewed(session);
                }
            }

            return RegisterSessionResult.Success(session.Token, session.Permissions);
        }
    }
}