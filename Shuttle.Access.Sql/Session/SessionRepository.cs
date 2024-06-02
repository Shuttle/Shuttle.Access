using System;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SessionRepository : ISessionRepository
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IDataRepository<Session> _dataRepository;
        private readonly ISessionQueryFactory _queryFactory;

        public SessionRepository(IDatabaseGateway databaseGateway, IDataRepository<Session> dataRepository,
            ISessionQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(dataRepository, nameof(dataRepository));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _databaseGateway = databaseGateway;
            _dataRepository = dataRepository;
            _queryFactory = queryFactory;
        }

        public Task SaveAsync(Session session, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(session, nameof(session));

            _databaseGateway.Execute(_queryFactory.Remove(session.IdentityName));
            _databaseGateway.Execute(_queryFactory.Add(session));

            foreach (var permission in session.Permissions)
            {
                _databaseGateway.Execute(_queryFactory.AddPermission(session.Token, permission));
            }
        }

        public Task RenewAsync(Session session, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(session, nameof(session));

            _databaseGateway.Execute(_queryFactory.Renew(session));
        }

        public Task<Session> GetAsync(Guid token, CancellationToken cancellationToken = default)
        {
            var result = FindAsync(token);

            if (result == null)
            {
                throw RecordNotFoundException.For("Session", token);
            }

            return result;
        }

        public Task<Session> FindAsync(Guid token, CancellationToken cancellationToken = default)
        {
            var session = _dataRepository.FetchItem(_queryFactory.Get(token));

            if (session == null)
            {
                return null;
            }

            foreach (var row in _databaseGateway.GetRows(_queryFactory.GetPermissions(token)))
            {
                session.AddPermission(Columns.PermissionName.Value(row));
            }

            return session;
        }

        public Task<Session> FindAsync(string identityName, CancellationToken cancellationToken = default)
        {
            var session = _dataRepository.FetchItem(_queryFactory.Get(identityName));

            if (session == null)
            {
                return null;
            }

            foreach (var row in _databaseGateway.GetRows(_queryFactory.GetPermissions(session.Token)))
            {
                session.AddPermission(Columns.PermissionName.Value(row));
            }

            return session;
        }

        public ValueTask<int> RemoveAsync(Guid token, CancellationToken cancellationToken = default)
        {
            return _databaseGateway.Execute(_queryFactory.Remove(token));
        }

        public ValueTask<int> RemoveAsync(string identityName, CancellationToken cancellationToken = default)
        {
            return _databaseGateway.Execute(_queryFactory.Remove(identityName));
        }
    }
}