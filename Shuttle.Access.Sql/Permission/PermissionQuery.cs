using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

public class PermissionQuery : IPermissionQuery
{
    private readonly IDatabaseContextService _databaseContextService;
    private readonly IPermissionQueryFactory _queryFactory;
    private readonly IQueryMapper _queryMapper;

    public PermissionQuery(IDatabaseContextService databaseContextService, IQueryMapper queryMapper, IPermissionQueryFactory queryFactory)
    {
        _databaseContextService = Guard.AgainstNull(databaseContextService);
        _queryMapper = Guard.AgainstNull(queryMapper);
        _queryFactory = Guard.AgainstNull(queryFactory);
    }

    public async Task<IEnumerable<DataAccess.Permission>> SearchAsync(DataAccess.Permission.Specification specification, CancellationToken cancellationToken = default)
    {
        return await _queryMapper.MapObjectsAsync<DataAccess.Permission>(_queryFactory.Search(specification), cancellationToken);
    }

    public async ValueTask<int> CountAsync(DataAccess.Permission.Specification specification, CancellationToken cancellationToken = default)
    {
        return await _databaseContextService.Active.GetScalarAsync<int>(_queryFactory.Count(specification), cancellationToken);
    }

    public async ValueTask<bool> ContainsAsync(DataAccess.Permission.Specification specification, CancellationToken cancellationToken = default)
    {
        return await _databaseContextService.Active.GetScalarAsync<int>(_queryFactory.Contains(specification), cancellationToken) == 1;
    }
}