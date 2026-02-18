using Microsoft.EntityFrameworkCore;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class IdentityQuery(AccessDbContext accessDbContext) : IIdentityQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<int> AdministratorCountAsync(CancellationToken cancellationToken = default)
    {
        return await _accessDbContext.IdentityRoles.CountAsync(item => item.Role.Name == "administrator", cancellationToken);
    }

    public async ValueTask<int> CountAsync(IdentitySpecification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification).CountAsync(cancellationToken);
    }

    public async ValueTask<Guid> IdAsync(string identityName, CancellationToken cancellationToken = default)
    {
        return (await _accessDbContext.Identities.FirstOrDefaultAsync(item => item.Name == identityName, cancellationToken)).GuardAgainstRecordNotFound(identityName).Id;
    }

    public async Task<IEnumerable<Models.Permission>> PermissionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return (await _accessDbContext.Identities.AsNoTracking()
                .Include(e => e.IdentityRoles)
                .ThenInclude(e => e.Role)
                .ThenInclude(e => e.RolePermissions)
                .ThenInclude(e => e.Permission)
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken))
            .GuardAgainstRecordNotFound(id)
            .IdentityRoles.SelectMany(role => role.Role.RolePermissions.Select(permission => permission.Permission))
            .ToList();
    }

    public async Task<IEnumerable<Guid>> RoleIdsAsync(IdentitySpecification specification, CancellationToken cancellationToken = default)
    {
        return (await SearchAsync(specification, cancellationToken))
            .SelectMany(e => e.IdentityRoles.Select(ir => ir.RoleId)).ToList();
    }

    public async Task<IEnumerable<Guid>> TenantIdsAsync(IdentitySpecification specification, CancellationToken cancellationToken = default)
    {
        return (await SearchAsync(specification, cancellationToken))
            .SelectMany(e => e.IdentityTenants.Select(it => it.TenantId)).ToList();
    }

    public async Task<IEnumerable<Models.Identity>> SearchAsync(IdentitySpecification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification)
            .OrderBy(e => e.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Models.Identity> GetQueryable(IdentitySpecification identitySpecification)
    {
        var queryable = _accessDbContext.Identities
            .Include(item => item.IdentityTenants)
            .ThenInclude(item => item.Tenant)
            .Include(item => item.IdentityRoles)
            .ThenInclude(item => item.Role)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(identitySpecification.NameMatch))
        {
            queryable = queryable.Where(e => e.Name.Contains(identitySpecification.NameMatch) || e.Description.Contains(identitySpecification.NameMatch));
        }

        if (!string.IsNullOrEmpty(identitySpecification.Name))
        {
            queryable = queryable.Where(e => e.Name == identitySpecification.Name || e.Description == identitySpecification.Name);
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

        return queryable;
    }
}