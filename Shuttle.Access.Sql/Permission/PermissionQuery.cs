using System.Collections.Generic;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class PermissionQuery : IPermissionQuery
    {
        private readonly IPermissionQueryFactory _queryFactory;
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IQueryMapper _queryMapper;

        public PermissionQuery(IDatabaseGateway databaseGateway, IQueryMapper queryMapper, IPermissionQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryMapper, nameof(queryMapper));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _databaseGateway = databaseGateway;
            _queryMapper = queryMapper;
            _queryFactory = queryFactory;
        }

        public IEnumerable<DataAccess.Query.Permission> Search(DataAccess.Query.Permission.Specification specification)
        {
            return _queryMapper.MapObjects<DataAccess.Query.Permission>(_queryFactory.Search(specification));
        }

        public int Count(DataAccess.Query.Permission.Specification specification)
        {
            return _databaseGateway.GetScalar<int>(_queryFactory.Count(specification));
        }

        public bool Contains(DataAccess.Query.Permission.Specification specification)
        {
            return _databaseGateway.GetScalar<int>(_queryFactory.Contains(specification)) == 1;
        }
    }
}