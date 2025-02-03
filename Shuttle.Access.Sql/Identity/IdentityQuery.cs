using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

public class IdentityQuery : IIdentityQuery
{
    private readonly IDatabaseContextService _databaseContextService;
    private readonly IIdentityQueryFactory _queryFactory;
    private readonly IQueryMapper _queryMapper;

    public IdentityQuery(IDatabaseContextService databaseContextService, IQueryMapper queryMapper, IIdentityQueryFactory queryFactory)
    {
        _databaseContextService = Guard.AgainstNull(databaseContextService);
        _queryFactory = Guard.AgainstNull(queryFactory);
        _queryMapper = Guard.AgainstNull(queryMapper);
    }

    public async ValueTask<int> AdministratorCountAsync(CancellationToken cancellationToken = default)
    {
        return await _databaseContextService.Active.GetScalarAsync<int>(_queryFactory.AdministratorCount(), cancellationToken);
    }

    public async ValueTask<Guid> IdAsync(string identityName, CancellationToken cancellationToken = default)
    {
        return await _databaseContextService.Active.GetScalarAsync<Guid>(_queryFactory.GetId(identityName), cancellationToken);
    }

    public async Task<IEnumerable<string>> PermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _queryMapper.MapValuesAsync<string>(_queryFactory.Permissions(userId), cancellationToken);
    }

    public async Task<IEnumerable<Messages.v1.Identity>> SearchAsync(DataAccess.Identity.Specification specification, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(specification);

        var result = await _queryMapper.MapObjectsAsync<Messages.v1.Identity>(_queryFactory.Search(specification), cancellationToken);

        if (specification.RolesIncluded)
        {
            var roleRows = await _databaseContextService.Active.GetRowsAsync(_queryFactory.Roles(specification), cancellationToken);

            foreach (var roleGroup in roleRows.GroupBy(row => Columns.IdentityId.Value(row)))
            {
                var user = result.FirstOrDefault(item => item.Id == roleGroup.Key);

                if (user == null)
                {
                    continue;
                }

                user.Roles = roleGroup.Select(row => new Messages.v1.Identity.Role
                    { Id = Columns.Id.Value(row), Name = Columns.Name.Value(row)! }).ToList();
            }
        }

        return result;
    }

    public async Task<IEnumerable<Guid>> RoleIdsAsync(DataAccess.Identity.Specification specification, CancellationToken cancellationToken = default)
    {
        return (await _databaseContextService.Active.GetRowsAsync(_queryFactory.Roles(Guard.AgainstNull(specification)), cancellationToken)).Select(row => Columns.Id.Value(row));
    }

    public async ValueTask<int> CountAsync(DataAccess.Identity.Specification specification, CancellationToken cancellationToken = default)
    {
        return await _databaseContextService.Active.GetScalarAsync<int>(_queryFactory.Count(Guard.AgainstNull(specification)), cancellationToken);
    }
}