using Microsoft.EntityFrameworkCore;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class RoleQuery(AccessDbContext accessDbContext) : IRoleQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<int> CountAsync(RoleSpecification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification).CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Models.Permission>> PermissionsAsync(RoleSpecification specification, CancellationToken cancellationToken = default)
    {
        var model = await GetQueryable(specification)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return model == null ? [] : model.RolePermissions.Select(e => e.Permission).ToList();
    }

    public async Task<IEnumerable<Models.Role>> SearchAsync(RoleSpecification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification)
            .OrderBy(e => e.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Models.Role> GetQueryable(RoleSpecification specification)
    {
        var queryable = _accessDbContext.Roles
            .Include(item => item.RolePermissions)
            .ThenInclude(item => item.Permission)
            .AsNoTracking()
            .AsQueryable();

        if (specification.TenantId.HasValue)
        {
            queryable = queryable.Where(e => e.TenantId == specification.TenantId.Value);
        }

        if (!string.IsNullOrEmpty(specification.NameMatch))
        {
            queryable = queryable.Where(e => EF.Functions.Like(e.Name, $"%{specification.NameMatch}%"));
        }

        if (specification.Names.Any())
        {
            queryable = queryable.Where(e => specification.Names.Contains(e.Name));
        }

        if (specification.PermissionIds.Any())
        {
            queryable = queryable.Where(e => e.RolePermissions.Any(rp => specification.PermissionIds.Contains(rp.PermissionId)));
        }

        if (specification.HasIds)
        {
            queryable = queryable.Where(e => specification.Ids.Contains(e.Id));
        }

        if (specification.HasExcludedIds)
        {
            queryable = queryable.Where(e => !specification.ExcludedIds.Contains(e.Id));
        }

        if (specification.MaximumRows > 0)
        {
            queryable = queryable.Take(specification.MaximumRows);
        }

        return queryable;
    }
}