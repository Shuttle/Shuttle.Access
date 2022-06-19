using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SessionQuery : ISessionQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IQueryMapper _queryMapper;
        private readonly ISessionQueryFactory _queryFactory;

        public SessionQuery(IDatabaseGateway databaseGateway, IQueryMapper queryMapper, ISessionQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryMapper, nameof(queryMapper));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _databaseGateway = databaseGateway;
            _queryMapper = queryMapper;
            _queryFactory = queryFactory;
        }

        public bool Contains(Guid token)
        {
            return _databaseGateway.GetScalarUsing<int>(_queryFactory.Contains(token)) == 1;
        }

        public bool Contains(Guid token, string permission)
        {
            return _databaseGateway.GetScalarUsing<int>(_queryFactory.Contains(token, permission)) == 1;
        }

        public DataAccess.Query.Session Get(Guid token)
        {
            var result = _queryMapper.MapObject<DataAccess.Query.Session>(_queryFactory.Get(token));

            result.GuardAgainstRecordNotFound(token);

            result.Permissions = _queryMapper.MapValues<string>(_queryFactory.GetPermissions(token)).ToList();

            return result;
        }

        public IEnumerable<DataAccess.Query.Session> Search(DataAccess.Query.Session.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return _queryMapper.MapObjects<DataAccess.Query.Session>(_queryFactory.Search(specification));
        }
    }
}