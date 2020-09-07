using System;
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

        public void Save(Session session)
        {
            Guard.AgainstNull(session, nameof(session));

            _databaseGateway.ExecuteUsing(_queryFactory.Remove(session.Username));
            _databaseGateway.ExecuteUsing(_queryFactory.Add(session));

            foreach (var permission in session.Permissions)
            {
                _databaseGateway.ExecuteUsing(_queryFactory.AddPermission(session.Token, permission));
            }
        }

        public Session Get(Guid token)
        {
            var result = Find(token);

            if (result == null)
            {
                throw RecordNotFoundException.For("Session", token);
            }

            return result;
        }

        public Session Find(Guid token)
        {
            var session = _dataRepository.FetchItemUsing(_queryFactory.Get(token));

            if (session == null)
            {
                return null;
            }

            foreach (var row in _databaseGateway.GetRowsUsing(_queryFactory.GetPermissions(token)))
            {
                session.AddPermission(Columns.Permission.MapFrom(row));
            }

            return session;
        }

        public int Remove(Guid token)
        {
            return _databaseGateway.ExecuteUsing(_queryFactory.Remove(token));
        }

        public int Remove(string username)
        {
            return _databaseGateway.ExecuteUsing(_queryFactory.Remove(username));
        }
    }
}