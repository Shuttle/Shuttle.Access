using Microsoft.EntityFrameworkCore;
using Shuttle.Contract;

namespace Shuttle.Access.SqlServer;

public class IdentityQuery(AccessDbContext accessDbContext) : IIdentityQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<int> AdministratorCountAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _accessDbContext.IdentityRoles.CountAsync(item => item.TenantId == tenantId && item.Role.Name == "Access Administrator", cancellationToken);
    }

    public async ValueTask<int> CountAsync(Query.Identity.Specification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification).CountAsync(cancellationToken);
    }

    public async ValueTask<Guid> IdAsync(string identityName, CancellationToken cancellationToken = default)
    {
        return (await _accessDbContext.Identities.FirstOrDefaultAsync(item => item.Name == identityName, cancellationToken)).GuardAgainstRecordNotFound(identityName).Id;
    }

    public async Task<IEnumerable<Query.Permission>> PermissionsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _accessDbContext.Identities.AsNoTracking()
            .Where(identity => identity.Id == id)
            .SelectMany(identity => identity.IdentityRoles
                .Where(identityRole => identityRole.TenantId == tenantId)
                .SelectMany(identityRole => identityRole.Role.RolePermissions
                    .Select(rolePermission => rolePermission.Permission)))
            .Select(permission => new Query.Permission
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description,
                Status = (PermissionStatus)permission.Status
            })
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Guid>> RoleIdsAsync(Query.Identity.Specification specification, CancellationToken cancellationToken = default)
    {
        return (await SearchAsync(specification, cancellationToken))
            .SelectMany(e => e.Roles.Select(item => item.Id)).ToList();
    }

    public async Task<IEnumerable<Guid>> TenantIdsAsync(Query.Identity.Specification specification, CancellationToken cancellationToken = default)
    {
        return (await SearchAsync(specification, cancellationToken))
            .SelectMany(e => e.Tenants.Select(item => item.Id)).ToList();
    }

    public async Task<IEnumerable<Query.Identity>> SearchAsync(Query.Identity.Specification specification, CancellationToken cancellationToken = default)
    {
        return (await GetQueryable(specification)
            .OrderBy(e => e.Name)
            .Distinct()
            .ToListAsync(cancellationToken))
            .Select(e => new Query.Identity
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                DateRegistered = e.DateRegistered,
                DateActivated = e.DateActivated,
                GeneratedPassword = e.GeneratedPassword,
                RegisteredBy = e.RegisteredBy,
                Tenants = e.IdentityTenants.Select(item => new Query.Tenant
                {
                    Id = item.TenantId,
                    Name = item.Tenant.Name,
                    Status = (TenantStatus)item.Tenant.Status,
                    LogoSvg = item.Tenant.LogoSvg,
                    LogoUrl = item.Tenant.LogoUrl
                }).ToList(),
                Roles = e.IdentityRoles.Select(item => new Query.Role
                {
                    Id = item.RoleId,
                    Name = item.Role.Name,
                    TenantId = item.Role.TenantId,
                    Permissions = specification.ShouldIncludePermissions
                        ? item.Role.RolePermissions.Select(rp => new Query.Permission
                        {
                            Id = rp.Permission.Id,
                            Name = rp.Permission.Name,
                            Status = (PermissionStatus)rp.Permission.Status,
                            Description = rp.Permission.Description
                        }).ToList()
                        : []
                }).ToList()
            });
    }

    private IQueryable<Models.Identity> GetQueryable(Query.Identity.Specification identitySpecification)
    {
        var queryable = _accessDbContext.Identities
            .AsNoTracking();

        queryable = identitySpecification.ShouldIncludeTenants
            ? queryable.Include(item => item.IdentityTenants).ThenInclude(item => item.Tenant)
            : queryable;

        queryable = identitySpecification is { ShouldIncludeRoles: true, ShouldIncludePermissions: false }
            ? queryable.Include(item => item.IdentityRoles).ThenInclude(item => item.Role)
            : queryable;

        queryable = identitySpecification.ShouldIncludePermissions
            ? queryable.Include(item => item.IdentityRoles).ThenInclude(item => item.Role).ThenInclude(item => item.RolePermissions).ThenInclude(item => item.Permission) 
            : queryable;

        if (!string.IsNullOrEmpty(identitySpecification.NameMatch))
        {
            queryable = queryable.Where(e => e.Name.Contains(identitySpecification.NameMatch) || e.Description.Contains(identitySpecification.NameMatch));
        }

        if (!string.IsNullOrEmpty(identitySpecification.Name))
        {
            queryable = queryable.Where(e => e.Name == identitySpecification.Name || e.Description == identitySpecification.Name);
        }

        if (identitySpecification.TenantId != null)
        {
            queryable = queryable.Where(e => e.IdentityTenants.Any(it => it.TenantId == identitySpecification.TenantId));
        }

        if (!string.IsNullOrEmpty(identitySpecification.RoleName))
        {
            queryable = queryable.Where(e => e.IdentityRoles.Any(ir => ir.Role.Name == identitySpecification.RoleName));
        }

        if (identitySpecification.RoleId != null)
        {
            queryable = queryable.Where(e => e.IdentityRoles.Any(ir => ir.RoleId == identitySpecification.RoleId));
        }

        if (identitySpecification.PermissionId != null)
        {
            queryable = queryable.Where(e => e.IdentityRoles.Any(ir => ir.Role.RolePermissions.Any(rp => rp.PermissionId == identitySpecification.PermissionId)));
        }

        if (identitySpecification.HasIds)
        {
            queryable = queryable.Where(e => identitySpecification.Ids.Contains(e.Id));
        }

        if (identitySpecification.HasExcludedIds)
        {
            queryable = queryable.Where(e => !identitySpecification.ExcludedIds.Contains(e.Id));
        }

        if (identitySpecification.MaximumRows > 0)
        {
            queryable = queryable.Take(identitySpecification.MaximumRows);
        }

        return queryable.AsSplitQuery();
    }
}