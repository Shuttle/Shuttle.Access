using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<DataAccess.Query.Role>> SearchAsync(DataAccess.Query.Role.Specification specification, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(specification, nameof(specification));

            var result = (await _queryMapper.MapObjectsAsync<DataAccess.Query.Role>(_queryFactory.Search(specification), cancellationToken)).ToList();

            if (specification.PermissionsIncluded)
            {
                var permissionRows = await _databaseGateway.GetRowsAsync(_queryFactory.Permissions(specification), cancellationToken);

                foreach (var permissionGroup in permissionRows.GroupBy(row=>Columns.RoleId.Value(row)))
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

        public async Task<IEnumerable<DataAccess.Query.Permission>> PermissionsAsync(DataAccess.Query.Role.Specification specification, CancellationToken cancellationToken = default)
        {
            return await _queryMapper.MapObjectsAsync<DataAccess.Query.Role.Permission>(_queryFactory.Permissions(specification), cancellationToken);
        }

        public async ValueTask<int> CountAsync(DataAccess.Query.Role.Specification specification, CancellationToken cancellationToken = default)
        {
            return await _databaseGateway.GetScalarAsync<int>(_queryFactory.Count(specification), cancellationToken);
        }
    }
}