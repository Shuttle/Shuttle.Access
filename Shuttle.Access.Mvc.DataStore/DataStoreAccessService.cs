using System;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Mvc.DataStore
{
    public class DataStoreAccessService : CachedAccessService, IAccessService
    {
        private readonly AccessOptions _accessOptions;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly ISessionRepository _sessionRepository;
        private readonly string _connectionStringName;

        public DataStoreAccessService(IOptionsMonitor<ConnectionStringOptions> connectionStringOptions, IOptions<AccessOptions> accessOptions, IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository) {
            Guard.AgainstNull(connectionStringOptions, nameof(connectionStringOptions));
            Guard.AgainstNull(accessOptions, nameof(accessOptions));
            Guard.AgainstNull(accessOptions.Value, nameof(accessOptions.Value));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));

            _accessOptions = accessOptions.Value;
            _connectionStringName = accessOptions.Value.ConnectionStringName;
            _databaseContextFactory = databaseContextFactory;
            _sessionRepository = sessionRepository;
        }

        public new bool Contains(Guid token)
        {
            var result = base.Contains(token);

            if (!result)
            {
                Cache(token);

                return base.Contains(token);
            }

            return true;
        }

        public new bool HasPermission(Guid token, string permission)
        {
            if (!base.Contains(token))
            {
                Cache(token);
            }

            return base.HasPermission(token, permission);
        }

        public new void Remove(Guid token)
        {
            base.Remove(token);
        }

        private void Cache(Guid token)
        {
            if (base.Contains(token))
            {
                return;
            }

            using (_databaseContextFactory.Create(_connectionStringName))
            {
                var session = _sessionRepository.FindAsync(token).GetAwaiter().GetResult();

                if (session != null)
                {
                    Cache(token, session.Permissions, _accessOptions.SessionDuration);
                }
            }
        }
    }
}