using System;
using Shuttle.Core.Contract;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Sql
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IEventStore _eventStore;
        private readonly IHashingService _hashingService;
        private readonly IKeyStore _keyStore;

        public AuthenticationService(IEventStore eventStore, IKeyStore keyStore, IHashingService hashingService)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(keyStore, nameof(keyStore));
            Guard.AgainstNull(hashingService, nameof(hashingService));

            _eventStore = eventStore;
            _keyStore = keyStore;
            _hashingService = hashingService;
        }

        public AuthenticationResult Authenticate(string identityName, string password)
        {
            var id = _keyStore.Get(Identity.Key(identityName));

            if (!id.HasValue)
            {
                Authentication.Invoke(this, new AuthenticationEventArgs(false, string.Format(Resources.InvalidIdentity, identityName)));

                return AuthenticationResult.Failure();
            }

            var identity = new Identity();

            _eventStore.Get(id.Value).Apply(identity);

            if (identity.PasswordMatches(_hashingService.Sha256(password)))
            {
                Authentication.Invoke(this, new AuthenticationEventArgs(true, string.Empty));

                return AuthenticationResult.Success();
            }

            Authentication.Invoke(this, new AuthenticationEventArgs(false, string.Format(Resources.InvalidPassword, identityName)));

            return AuthenticationResult.Failure();
        }

        public AuthenticationResult Authenticate(string identityName)
        {
            return AuthenticationResult.Success();
        }

        public event EventHandler<AuthenticationEventArgs> Authentication = delegate
        {
        };
    }
}