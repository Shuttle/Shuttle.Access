using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Mvc.DataStore
{
    public class DataStoreAccessService : CachedAccessService, IAccessService
    {
        private readonly IAccessConnectionConfiguration _connectionConfiguration;
        private readonly IAccessSessionConfiguration _sessionConfiguration;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly ISessionRepository _sessionRepository;

        public DataStoreAccessService(IAccessConnectionConfiguration connectionConfiguration, IAccessSessionConfiguration sessionConfiguration, IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository) {
            Guard.AgainstNull(connectionConfiguration, nameof(connectionConfiguration));
            Guard.AgainstNull(sessionConfiguration, nameof(sessionConfiguration));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));

            _connectionConfiguration = connectionConfiguration;
            _sessionConfiguration = sessionConfiguration;
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

            using (_databaseContextFactory.Create(_connectionConfiguration.ProviderName, _connectionConfiguration.ConnectionString))
            {
                var session = _sessionRepository.Find(token);

                if (session != null)
                {
                    Cache(token, session.Permissions, _sessionConfiguration.SessionDuration);
                }
            }
        }
    }
}