using Microsoft.EntityFrameworkCore;
using Shuttle.Access.SqlServer.Models;
using Shuttle.Contract;
using static System.Net.Mime.MediaTypeNames;
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
                .Include(e => e.Permissions).ThenInclude(e => e.Permission)
                .Include(e => e.Tokens)
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
                Permissions = e.Permissions.Select(item => new Query.Session.SessionPermission
                {
                    Id = item.PermissionId,
                    Name = item.Permission.Name,
                    TenantId = item.TenantId
                }).ToList(),
                Tokens = e.Tokens.Select(item => new Query.Session.SessionToken
                {
                    Id = item.Id,
                    TokenHash = item.TokenHash,
                    DateRegistered = item.DateRegistered,
                    ExpiryDate = item.ExpiryDate,
                    Application = item.Application
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
            .Include(item => item.Permissions)
            .Include(item => item.Tokens)
            .SingleOrDefaultAsync(item => item.Id == session.Id, cancellationToken);

        if (model == null)
        {
            model = new()
            {
                Id = session.Id,
                IdentityId = session.IdentityId,
                DateRegistered = session.DateRegistered,
                ExpiryDate = session.ExpiryDate
            };

            _accessDbContext.Sessions.Add(model);
        }
        else
        {
            _accessDbContext.SessionPermissions
                .RemoveRange(model.Permissions.Where(item => session.Permissions.All(p => p.Id != item.PermissionId && p.TenantId != item.TenantId)).ToList());

            _accessDbContext.SessionTokens
                .RemoveRange(model.Tokens.Where(item => session.Tokens.All(t => t.Id != item.Id)).ToList());
        }

        model.ExpiryDate = session.ExpiryDate;

        foreach (var sessionPermission in session.Permissions)
        {
            var sessionPermissionModel = model.Permissions.FirstOrDefault(e => e.PermissionId == sessionPermission.Id);

            if (sessionPermissionModel != null)
            {
                continue;
            }

            _accessDbContext.SessionPermissions.Add(new()
            {
                SessionId = session.Id,
                PermissionId = sessionPermission.Id,
                TenantId = sessionPermission.TenantId
            });
        }

        foreach (var sessionToken in session.Tokens)
        {
            var sessionTokenModel = model.Tokens.FirstOrDefault(e => e.Id == sessionToken.Id);

            if (sessionTokenModel == null)
            {
                sessionTokenModel = new()
                {
                    Id = sessionToken.Id
                };

                _accessDbContext.SessionTokens.Add(sessionTokenModel);
            }

            sessionTokenModel.SessionId = session.Id;
            sessionTokenModel.TokenHash = sessionToken.TokenHash;
            sessionTokenModel.DateRegistered = sessionToken.DateRegistered;
            sessionTokenModel.ExpiryDate = sessionToken.ExpiryDate;
            sessionTokenModel.Application = sessionToken.Application;
        }

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
            queryable = queryable.Where(e => e.Tokens.Any(t=> t.TokenHash == specification.TokenHash));
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
            queryable = queryable.Where(e => e.Tokens.Any(t => t.Application == specification.Application));
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
            queryable = queryable.Where(e => e.Permissions.Any(p => specification.Permissions.Contains(p.Permission.Name)));
        }

        return queryable;
    }
}