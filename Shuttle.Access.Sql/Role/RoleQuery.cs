using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

public class RoleQuery : IRoleQuery
{
    private readonly IDatabaseContextService _databaseContextService;
    private readonly IDataRowMapper _dataRowMapper;
    private readonly IRoleQueryFactory _queryFactory;
    private readonly IQueryMapper _queryMapper;

    public RoleQuery(IDatabaseContextService databaseContextService, IQueryMapper queryMapper, IDataRowMapper dataRowMapper, IRoleQueryFactory queryFactory)
    {
        _databaseContextService = Guard.AgainstNull(databaseContextService);
        _queryFactory = Guard.AgainstNull(queryFactory);
        _queryMapper = Guard.AgainstNull(queryMapper);
        _dataRowMapper = Guard.AgainstNull(dataRowMapper);
    }

    public async Task<IEnumerable<Messages.v1.Role>> SearchAsync(DataAccess.Role.Specification specification, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(specification);

        var result = (await _queryMapper.MapObjectsAsync<Messages.v1.Role>(_queryFactory.Search(specification), cancellationToken)).ToList();

        if (specification.PermissionsIncluded)
        {
            var permissionRows = await _databaseContextService.Active.GetRowsAsync(_queryFactory.Permissions(specification), cancellationToken);

            foreach (var permissionGroup in permissionRows.GroupBy(row => Columns.RoleId.Value(row)))
            {
                var role = result.FirstOrDefault(item => item.Id == permissionGroup.Key);

                if (role == null)
                {
                    continue;
                }

                role.Permissions = _dataRowMapper.MapObjects<Messages.v1.Role.Permission>(permissionGroup).ToList();
            }
        }

        return result;
    }

    public async Task<IEnumerable<Messages.v1.Permission>> PermissionsAsync(DataAccess.Role.Specification specification, CancellationToken cancellationToken = default)
    {
        return await _queryMapper.MapObjectsAsync<Messages.v1.Role.Permission>(_queryFactory.Permissions(specification), cancellationToken);
    }

    public async ValueTask<int> CountAsync(DataAccess.Role.Specification specification, CancellationToken cancellationToken = default)
    {
        return await _databaseContextService.Active.GetScalarAsync<int>(_queryFactory.Count(specification), cancellationToken);
    }
}