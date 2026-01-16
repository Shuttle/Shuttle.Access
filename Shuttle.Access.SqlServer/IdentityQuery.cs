using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class IdentityQuery(AccessDbContext accessDbContext) : IIdentityQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<int> AdministratorCountAsync(CancellationToken cancellationToken = default)
    {
        return await _accessDbContext.IdentityRoles.CountAsync(item => item.Role.Name == "administrator", cancellationToken);
    }

    public async ValueTask<int> CountAsync(Models.Identity.Specification specification, CancellationToken cancellationToken = default)
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

    public async Task<IEnumerable<Guid>> RoleIdsAsync(Models.Identity.Specification specification, CancellationToken cancellationToken = default)
    {
        return (await SearchAsync(specification, cancellationToken))
            .SelectMany(e => e.IdentityRoles.Select(ir => ir.RoleId)).ToList();
    }

    public async Task<IEnumerable<Models.Identity>> SearchAsync(Models.Identity.Specification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification)
            .OrderBy(e => e.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Models.Identity> GetQueryable(Models.Identity.Specification specification)
    {
        var queryable = _accessDbContext.Identities
            .Include(item => item.IdentityTenants)
            .Include(item => item.IdentityRoles)
            .ThenInclude(item => item.Role)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(specification.NameMatch))
        {
            queryable = queryable.Where(e => e.Name.Contains(specification.NameMatch) || e.Description.Contains(specification.NameMatch));
        }

        if (!string.IsNullOrEmpty(specification.Name))
        {
            queryable = queryable.Where(e => e.Name == specification.Name || e.Description == specification.Name);
        }

        if (!string.IsNullOrEmpty(specification.RoleName))
        {
            queryable = queryable.Where(e => e.IdentityRoles.Any(ir => ir.Role.Name == specification.RoleName));
        }

        if (specification.RoleId != null)
        {
            queryable = queryable.Where(e => e.IdentityRoles.Any(ir => ir.RoleId == specification.RoleId));
        }

        if (specification.PermissionId != null)
        {
            queryable = queryable.Where(e => e.IdentityRoles.Any(ir => ir.Role.RolePermissions.Any(rp => rp.PermissionId == specification.PermissionId)));
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