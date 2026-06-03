using Microsoft.EntityFrameworkCore;
using Shuttle.Access.SqlServer.Models;
using Shuttle.Contract;
using Session = Shuttle.Access.Query.Session;

namespace Shuttle.Access.SqlServer;

public class SessionQuery(AccessDbContext accessDbContext, IHashingService hashingService) : ISessionQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<int> CountAsync(Session.Specification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification).CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Session>> SearchAsync(Session.Specification specification, CancellationToken cancellationToken = default)
    {
        return (await GetQueryable(specification)
                .Include(e => e.Identity).ThenInclude(e => e.IdentityRoles)
                .Include(e => e.SessionPermissions).ThenInclude(e => e.Permission)
                .OrderBy(e => e.Identity.Name)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync(cancellationToken))
            .Select(e => new Session
            {
                Id = e.Id,
                DateRegistered = e.DateRegistered,
                ExpiryDate = e.ExpiryDate,
                IdentityId = e.IdentityId,
                IdentityName = e.Identity.Name,
                IdentityDescription = e.Identity.Description,
                TokenHash = e.TokenHash,
                Permissions = e.SessionPermissions.Select(item => new Query.Session.Permission
                {
                    Id = item.PermissionId,
                    Name = item.Permission.Name,
                    TenantId = item.TenantId
                }).ToList()
            });
    }

    public async ValueTask<int> RemoveAsync(Session.Specification specification, CancellationToken cancellationToken = default)
    {
        if (!specification.HasCriteria)
        {
            return await _accessDbContext.Database.ExecuteSqlAsync($"DELETE FROM [access].[Session]", cancellationToken);
        }

        return await GetQueryable(specification).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task SaveAsync(Session session, CancellationToken cancellationToken = default)
    {
        var model = await _accessDbContext.Sessions
            .Include(item => item.SessionPermissions)
            .SingleOrDefaultAsync(item => item.Id == session.Id, cancellationToken);

        if (model == null)
        {
            model = new()
            {
                Id = session.Id,
                IdentityId = session.IdentityId,
                DateRegistered = session.DateRegistered,
                ExpiryDate = session.ExpiryDate,
                TokenHash = session.TokenHash,
                Application = session.Application,
                SessionPermissions = session.Permissions.Select(p => new SessionPermission
                {
                    SessionId = session.Id,
                    PermissionId = p.Id,
                    TenantId = p.TenantId
                }).ToList()
            };

            _accessDbContext.Sessions.Add(model);
        }
        else
        {
            _accessDbContext.SessionPermissions.RemoveRange(model.SessionPermissions);
        }

        model.TokenHash = session.TokenHash;
        model.ExpiryDate = session.ExpiryDate;
        model.SessionPermissions.Clear();

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        model.SessionPermissions = session.Permissions.Select(p => new SessionPermission
        {
            SessionId = session.Id,
            PermissionId = p.Id,
            TenantId = p.TenantId
        }).ToList();

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Models.Session> GetQueryable(Session.Specification specification)
    {
        var queryable = _accessDbContext.Sessions.AsQueryable();

        if (specification.Token.HasValue)
        {
            specification.WithTokenHash(hashingService.Sha256($"{specification.Token.Value:D}"));
        }

        if (!string.IsNullOrWhiteSpace(specification.TokenHash))
        {
            queryable = queryable.Where(e => e.TokenHash == specification.TokenHash);
        }

        if (specification.HasIds)
        {
            queryable = queryable.Where(e => specification.Ids.Contains(e.Id));
        }

        if (specification.HasExcludedIds)
        {
            queryable = queryable.Where(e => !specification.ExcludedIds.Contains(e.Id));
        }

        if (!string.IsNullOrWhiteSpace(specification.Application))
        {
            queryable = queryable.Where(e => e.Application == specification.Application);
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
            queryable = queryable.Where(e => e.Identity.Name == specification.IdentityName);
        }

        if (!string.IsNullOrWhiteSpace(specification.IdentityNameMatch))
        {
            queryable = queryable.Where(e => e.Identity.Name.Contains(specification.IdentityNameMatch));
        }

        if (specification.Permissions.Any())
        {
            queryable = queryable.Where(e => e.SessionPermissions.Any(p => specification.Permissions.Contains(p.Permission.Name)));
        }

        return queryable;
    }
}