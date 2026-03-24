using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class RoleQuery(AccessDbContext accessDbContext) : IRoleQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<int> CountAsync(Query.Role.Specification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification).CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Query.Permission>> PermissionsAsync(Query.Role.Specification specification, CancellationToken cancellationToken = default)
    {
        var model = await GetQueryable(specification)
            .FirstOrDefaultAsync(cancellationToken);

        return model == null
            ? []
            : model.RolePermissions.Select(e => e.Permission).Select(e => new Query.Permission
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Status = (PermissionStatus)e.Status
            }).ToList();
    }

    public async Task<IEnumerable<Query.Role>> SearchAsync(Query.Role.Specification specification, CancellationToken cancellationToken = default)
    {
        return (await GetQueryable(specification)
                .OrderBy(e => e.Name)
                .ToListAsync(cancellationToken))
            .Select(e => new Query.Role
            {
                Id = e.Id,
                Name = e.Name,
                TenantId = e.TenantId,
                Permissions = e.RolePermissions.Select(rolePermission => rolePermission.Permission).Select(permission => new Query.Permission
                {
                    Id = permission.Id,
                    Name = permission.Name,
                    Description = permission.Description,
                    Status = (PermissionStatus)permission.Status
                }).ToList(),
                Identities = e.IdentityRoles.Select(identityRole => identityRole.Identity).Select(identity => new Query.Role.Identity
                {
                    Id = identity.Id,
                    Name = identity.Name,
                    Description = identity.Description
                }).ToList()
            });
    }

    private IQueryable<Models.Role> GetQueryable(Query.Role.Specification specification)
    {
        var queryable = _accessDbContext.Roles
            .AsNoTracking()
            .Include(item => item.RolePermissions).ThenInclude(item => item.Permission)
            .Include(item => item.IdentityRoles).ThenInclude(item => item.Identity)
            .AsSplitQuery();

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