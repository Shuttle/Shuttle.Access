using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class PermissionQuery(AccessDbContext accessDbContext) : IPermissionQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<int> CountAsync(Query.Permission.Specification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification).CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Query.Permission>> SearchAsync(Query.Permission.Specification specification, CancellationToken cancellationToken = default)
    {
        return (await GetQueryable(specification)
            .Distinct()
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken))
            .Select(e => new Query.Permission
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Status =(PermissionStatus) e.Status
            });
    }

    private IQueryable<Models.Permission> GetQueryable(Query.Permission.Specification permissionSpecification)
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