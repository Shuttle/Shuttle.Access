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

    private IQueryable<Models.Role> GetQueryable(RoleSpecification roleSpecification)
    {
        var queryable = _accessDbContext.Roles
            .Include(item => item.RolePermissions)
            .ThenInclude(item => item.Permission)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(roleSpecification.NameMatch))
        {
            queryable = queryable.Where(e => EF.Functions.Like(e.Name, $"%{roleSpecification.NameMatch}%"));
        }

        if (roleSpecification.Names.Any())
        {
            queryable = queryable.Where(e => roleSpecification.Names.Contains(e.Name));
        }

        if (roleSpecification.PermissionIds.Any())
        {
            queryable = queryable.Where(e => e.RolePermissions.Any(rp => roleSpecification.PermissionIds.Contains(rp.PermissionId)));
        }

        if (roleSpecification.HasIds)
        {
            queryable = queryable.Where(e => roleSpecification.Ids.Contains(e.Id));
        }

        if (roleSpecification.HasExcludedIds)
        {
            queryable = queryable.Where(e => !roleSpecification.ExcludedIds.Contains(e.Id));
        }

        if (roleSpecification.MaximumRows > 0)
        {
            queryable = queryable.Take(roleSpecification.MaximumRows);
        }

        return queryable;
    }
}