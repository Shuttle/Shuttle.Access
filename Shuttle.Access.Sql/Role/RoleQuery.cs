using System.Collections.Generic;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class RoleQuery : IRoleQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IRoleQueryFactory _queryFactory;
        private readonly IQueryMapper _queryMapper;
        private readonly IDataRowMapper _dataRowMapper;

        public RoleQuery(IDatabaseGateway databaseGateway, IQueryMapper queryMapper, IDataRowMapper dataRowMapper, IRoleQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));
            Guard.AgainstNull(queryMapper, nameof(queryMapper));
            Guard.AgainstNull(dataRowMapper, nameof(dataRowMapper));

            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
            _queryMapper = queryMapper;
            _dataRowMapper = dataRowMapper;
        }

        public IEnumerable<DataAccess.Query.Role> Search(DataAccess.Query.Role.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            var result = _queryMapper.MapObjects<DataAccess.Query.Role>(_queryFactory.Search(specification)).ToList();

            if (specification.PermissionsIncluded)
            {
                var permissionRows = _databaseGateway.GetRows(_queryFactory.Permissions(specification));

                foreach (var permissionGroup in permissionRows.GroupBy(row=>Columns.RoleId.MapFrom(row)))
                {
                    var role = result.FirstOrDefault(item => item.Id == permissionGroup.Key);

                    if (role == null)
                    {
                        continue;
                    }

                    role.Permissions = _dataRowMapper.MapObjects<DataAccess.Query.Role.Permission>(permissionGroup).ToList();
                }
            }

            return result;
        }

        public IEnumerable<DataAccess.Query.Role.Permission> Permissions(DataAccess.Query.Role.Specification specification)
        {
            return _queryMapper.MapObjects<DataAccess.Query.Role.Permission>(_queryFactory.Permissions(specification));
        }

        public int Count(DataAccess.Query.Role.Specification specification)
        {
            return _databaseGateway.GetScalar<int>(_queryFactory.Count(specification));
        }
    }
}