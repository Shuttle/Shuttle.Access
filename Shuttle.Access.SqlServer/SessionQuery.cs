using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class SessionQuery(AccessDbContext accessDbContext, IHashingService hashingService) : ISessionQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<int> CountAsync(Query.Session.Specification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification).CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Query.Session>> SearchAsync(Query.Session.Specification specification, CancellationToken cancellationToken = default)
    {
        return (await GetQueryable(specification)
            .Include(e => e.Identity).ThenInclude(e => e.IdentityRoles)
            .Include(e => e.SessionPermissions)
            .ThenInclude(e => e.Permission)
            .OrderBy(e => e.IdentityName)
            .Distinct()
            .ToListAsync(cancellationToken))
            .Select(e => new Query.Session
            {
                Id = e.Id,
                DateRegistered = e.DateRegistered,
                ExpiryDate = e.ExpiryDate,
                IdentityId = e.IdentityId,
                IdentityName = e.IdentityName,
                IdentityDescription = e.Identity.Description,
                TokenHash = e.Token,
                TenantId = e.TenantId,
                TenantName = e.Tenant?.Name ?? string.Empty,
                Permissions = e.SessionPermissions.Select(item => new Query.Permission
                {
                    Id = item.PermissionId,
                    Name = item.Permission.Name,
                    Description = item.Permission.Description,
                    Status = (PermissionStatus)item.Permission.Status
                }).ToList()
            })
            ;
    }

    private IQueryable<Models.Session> GetQueryable(Query.Session.Specification specification)
    {
        var queryable = _accessDbContext.Sessions.AsQueryable();

        if (specification.HasNullTenantId || specification.TenantId.HasValue)
        {
            queryable = queryable.Where(e => e.TenantId == specification.TenantId);
        }

        if (specification.Token.HasValue)
        {
            specification.WithTokenHash(hashingService.Sha256($"{specification.Token.Value:D}"));
        }

        if (specification.TokenHash != null)
        {
            queryable = queryable.Where(e => e.Token == specification.TokenHash);
        }

        if (specification.IdentityId.HasValue)
        {
            queryable = queryable.Where(e => e.IdentityId == specification.IdentityId);
        }

        if (specification.RoleId.HasValue)
        {
            queryable = queryable.Where(e => e.Identity.IdentityRoles.Any(item => item.RoleId == specification.RoleId.Value));
        }

        if (!string.IsNullOrWhiteSpace(specification.IdentityName))
        {
            queryable = queryable.Where(e => e.IdentityName == specification.IdentityName);
        }

        if (!string.IsNullOrWhiteSpace(specification.IdentityNameMatch))
        {
            queryable = queryable.Where(e => e.IdentityName.Contains(specification.IdentityNameMatch));
        }

        if (specification.Permissions.Any())
        {
            queryable = queryable.Where(e => e.SessionPermissions.Any(p => specification.Permissions.Contains(p.Permission.Name)));
        }

        return queryable;
    }
}