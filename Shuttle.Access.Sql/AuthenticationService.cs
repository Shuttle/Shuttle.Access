using System;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;
using Shuttle.Recall;

namespace Shuttle.Access.Sql
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IEventStore _eventStore;
        private readonly IHashingService _hashingService;
        private readonly IKeyStore _keyStore;
        private readonly ILog _log;

        public AuthenticationService(IDatabaseContextFactory databaseContextFactory, IEventStore eventStore,
            IKeyStore keyStore, IHashingService hashingService)
        {
            Guard.AgainstNull(databaseContextFactory, "databaseContextFactory");
            Guard.AgainstNull(eventStore, "eventStore");
            Guard.AgainstNull(keyStore, "keyStore");
            Guard.AgainstNull(hashingService, "hashingService");

            _databaseContextFactory = databaseContextFactory;
            _eventStore = eventStore;
            _keyStore = keyStore;
            _hashingService = hashingService;

            _log = Log.For(this);
        }

        public AuthenticationResult Authenticate(string username, string password)
        {
            EventStream stream;
            Guid? userId;

            using (_databaseContextFactory.Create())
            {
                userId = _keyStore.Get(User.Key(username));

                if (!userId.HasValue)
                {
                    _log.Trace(string.Format("[username not found] : username = '{0}'", username));

                    return AuthenticationResult.Failure();
                }

                stream = _eventStore.Get(userId.Value);
            }

            var user = new User(userId.Value);

            stream.Apply(user);

            if (user.PasswordMatches(_hashingService.Sha256(password)))
            {
                return AuthenticationResult.Success();
            }

            _log.Trace(string.Format("[invalid password] : username = '{0}'", username));

            return AuthenticationResult.Failure();
        }
    }
}