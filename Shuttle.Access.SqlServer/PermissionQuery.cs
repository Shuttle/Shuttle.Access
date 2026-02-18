using Microsoft.EntityFrameworkCore;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class PermissionQuery(AccessDbContext accessDbContext) : IPermissionQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<int> CountAsync(PermissionSpecification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification).CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Models.Permission>> SearchAsync(PermissionSpecification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification)
            .Distinct()
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Models.Permission> GetQueryable(PermissionSpecification permissionSpecification)
    {
        var queryable = _accessDbContext.Permissions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(permissionSpecification.NameMatch))
        {
            queryable = queryable.Where(e => EF.Functions.Like(e.Name, $"%{permissionSpecification.NameMatch}%"));
        }

        if (permissionSpecification.Names.Any())
        {
            queryable = queryable.Where(e => permissionSpecification.Names.Contains(e.Name));
        }

        if (permissionSpecification.Ids.Any())
        {
            queryable = queryable.Where(e => permissionSpecification.Ids.Contains(e.Id));
        }

        if (permissionSpecification.RoleIds.Any())
        {
            queryable = queryable.Where(e => e.RolePermissions.Any(rp => permissionSpecification.RoleIds.Contains(rp.RoleId)));
        }

        return queryable;
    }
}