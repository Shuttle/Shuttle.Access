using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class RoleQuery(AccessDbContext accessDbContext) : IRoleQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<bool> ContainsAsync(Models.Role.Specification specification, CancellationToken cancellationToken = default)
    {
        return await CountAsync(specification, cancellationToken) > 0;
    }

    public async ValueTask<int> CountAsync(Models.Role.Specification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification).CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Models.Permission>> PermissionsAsync(Models.Role.Specification specification, CancellationToken cancellationToken = default)
    {
        var model = await GetQueryable(specification)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return model == null ? [] : model.RolePermissions.Select(e => e.Permission).ToList();
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
        var queryable = _accessDbContext.Roles
            .Include(item => item.RolePermissions)
            .ThenInclude(item => item.Permission)
            .AsNoTracking()
            .AsQueryable();

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