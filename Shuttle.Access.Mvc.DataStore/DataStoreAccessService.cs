using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Mvc.DataStore
{
    public class DataStoreAccessService : CachedAccessService, IAccessService
    {
        private readonly IAccessConfiguration _configuration;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly ISessionRepository _sessionRepository;

        public DataStoreAccessService(IAccessConfiguration configuration,
            IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));

            _configuration = configuration;
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

            using (_databaseContextFactory.Create(_configuration.ProviderName, _configuration.ConnectionString))
            {
                var session = _sessionRepository.Find(token);

                if (session != null)
                {
                    Cache(token, session.Permissions, _configuration.SessionDuration);
                }
            }
        }
    }
}