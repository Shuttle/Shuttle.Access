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

            _databaseGateway.Execute(_queryFactory.Remove(session.IdentityName));
            _databaseGateway.Execute(_queryFactory.Add(session));

            foreach (var permission in session.Permissions)
            {
                _databaseGateway.Execute(_queryFactory.AddPermission(session.Token, permission));
            }
        }

        public void Renew(Session session)
        {
            Guard.AgainstNull(session, nameof(session));

            _databaseGateway.Execute(_queryFactory.Renew(session));
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
            var session = _dataRepository.FetchItem(_queryFactory.Get(token));

            if (session == null)
            {
                return null;
            }

            foreach (var row in _databaseGateway.GetRows(_queryFactory.GetPermissions(token)))
            {
                session.AddPermission(Columns.PermissionName.MapFrom(row));
            }

            return session;
        }

        public Session Find(string identityName)
        {
            var session = _dataRepository.FetchItem(_queryFactory.Get(identityName));

            if (session == null)
            {
                return null;
            }

            foreach (var row in _databaseGateway.GetRows(_queryFactory.GetPermissions(session.Token)))
            {
                session.AddPermission(Columns.PermissionName.MapFrom(row));
            }

            return session;
        }

        public int Remove(Guid token)
        {
            return _databaseGateway.Execute(_queryFactory.Remove(token));
        }

        public int Remove(string identityName)
        {
            return _databaseGateway.Execute(_queryFactory.Remove(identityName));
        }
    }
}