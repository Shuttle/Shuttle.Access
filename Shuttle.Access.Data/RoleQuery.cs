using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Data;

public class RoleQuery(AccessDbContext accessDbContext) : IRoleQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<int> CountAsync(Models.Role.Specification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification).CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Models.Permission>> PermissionsAsync(Models.Role.Specification specification, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Models.Role>> SearchAsync(Models.Role.Specification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification)
            .OrderBy(e => e.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Models.Role> GetQueryable(Models.Role.Specification specification)
    {
        var queryable = _accessDbContext.Roles.AsQueryable();

        if (!string.IsNullOrEmpty(specification.NameMatch))
        {
            queryable = queryable.Where(e => EF.Functions.Like(e.Name, $"%{specification.NameMatch}%"));
        }

        if (specification.Names.Any())
        {
            queryable = queryable.Where(e => specification.Names.Contains(e.Name));
        }

        if (specification.RoleIds.Any())
        {
            queryable = queryable.Where(e => specification.RoleIds.Contains(e.Id));
        }

        if (specification.PermissionIds.Any())
        {
            queryable = queryable.Where(e => e.RolePermissions.Any(rp => specification.PermissionIds.Contains(rp.PermissionId)));
        }

        return queryable;
    }
}