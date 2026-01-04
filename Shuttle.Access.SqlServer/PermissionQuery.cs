using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class PermissionQuery(AccessDbContext accessDbContext) : IPermissionQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<bool> ContainsAsync(Models.Permission.Specification specification, CancellationToken cancellationToken = default)
    {
        return await CountAsync(specification, cancellationToken) > 0;
    }

    public async ValueTask<int> CountAsync(Models.Permission.Specification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification).CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Models.Permission>> SearchAsync(Models.Permission.Specification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification)
            .OrderBy(e => e.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Models.Permission> GetQueryable(Models.Permission.Specification specification)
    {
        var queryable = _accessDbContext.Permissions.AsQueryable();

        if (specification.Names.Any())
        {
            queryable = queryable.Where(e => specification.Names.Contains(e.Name));
        }

        if (specification.Ids.Any())
        {
            queryable = queryable.Where(e => specification.Ids.Contains(e.Id));
        }

        if (specification.RoleIds.Any())
        {
            queryable = queryable.Where(e => e.RolePermissions.Any(rp => specification.RoleIds.Contains(rp.RoleId)));
        }

        return queryable;
    }
}