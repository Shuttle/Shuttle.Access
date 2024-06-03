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

        public async Task SaveAsync(Session session, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(session, nameof(session));

            await _databaseGateway.ExecuteAsync(_queryFactory.Remove(session.IdentityName), cancellationToken);
            await _databaseGateway.ExecuteAsync(_queryFactory.Add(session), cancellationToken);

            foreach (var permission in session.Permissions)
            {
                await _databaseGateway.ExecuteAsync(_queryFactory.AddPermission(session.Token, permission), cancellationToken);
            }
        }

        public async Task RenewAsync(Session session, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(session, nameof(session));

            await _databaseGateway.ExecuteAsync(_queryFactory.Renew(session), cancellationToken);
        }

        public async Task<Session> GetAsync(Guid token, CancellationToken cancellationToken = default)
        {
            var result = await FindAsync(token, cancellationToken);

            if (result == null)
            {
                throw RecordNotFoundException.For("Session", token);
            }

            return result;
        }

        public async Task<Session> FindAsync(Guid token, CancellationToken cancellationToken = default)
        {
            var session = await _dataRepository.FetchItemAsync(_queryFactory.Get(token), cancellationToken);

            if (session == null)
            {
                return null;
            }

            foreach (var row in await _databaseGateway.GetRowsAsync(_queryFactory.GetPermissions(token), cancellationToken))
            {
                session.AddPermission(Columns.PermissionName.Value(row));
            }

            return session;
        }

        public async Task<Session> FindAsync(string identityName, CancellationToken cancellationToken = default)
        {
            var session = await _dataRepository.FetchItemAsync(_queryFactory.Get(identityName), cancellationToken);

            if (session == null)
            {
                return null;
            }

            foreach (var row in await _databaseGateway.GetRowsAsync(_queryFactory.GetPermissions(session.Token), cancellationToken))
            {
                session.AddPermission(Columns.PermissionName.Value(row));
            }

            return session;
        }

        public async ValueTask<int> RemoveAsync(Guid token, CancellationToken cancellationToken = default)
        {
            return await _databaseGateway.ExecuteAsync(_queryFactory.Remove(token), cancellationToken);
        }

        public async ValueTask<int> RemoveAsync(string identityName, CancellationToken cancellationToken = default)
        {
            return await _databaseGateway.ExecuteAsync(_queryFactory.Remove(identityName), cancellationToken);
        }
    }
}