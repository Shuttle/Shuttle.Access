using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Sql
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAccessConfiguration _configuration;
        private readonly IEventStore _eventStore;
        private readonly IHashingService _hashingService;
        private readonly IKeyStore _keyStore;
        private readonly ILog _log;

        public AuthenticationService(IAccessConfiguration configuration,
            IEventStore eventStore, IKeyStore keyStore, IHashingService hashingService)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(keyStore, nameof(keyStore));
            Guard.AgainstNull(hashingService, nameof(hashingService));

            _configuration = configuration;
            _eventStore = eventStore;
            _keyStore = keyStore;
            _hashingService = hashingService;

            _log = Log.For(this);
        }

        public AuthenticationResult Authenticate(string username, string password)
        {
            EventStream stream;
            Guid? userId;

            userId = _keyStore.Get(Identity.Key(username));

            if (!userId.HasValue)
            {
                _log.Trace($"[identityName not found] : identityName = '{username}'");

                return AuthenticationResult.Failure();
            }

            stream = _eventStore.Get(userId.Value);

            var user = new Identity(userId.Value);

            stream.Apply(user);

            if (user.PasswordMatches(_hashingService.Sha256(password)))
            {
                return AuthenticationResult.Success();
            }

            _log.Trace($"[invalid password] : identityName = '{username}'");

            return AuthenticationResult.Failure();
        }

        public AuthenticationResult Authenticate(string username)
        {
            return AuthenticationResult.Success();
        }
    }
}