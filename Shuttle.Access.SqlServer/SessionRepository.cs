using Microsoft.EntityFrameworkCore;
using Shuttle.Access.Query;
using Shuttle.Access.SqlServer.Models;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class SessionRepository(AccessDbContext accessDbContext) : ISessionRepository
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async Task<IEnumerable<Session>> SearchAsync(SessionSpecification specification, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(specification);

        var results = new List<Session>();

        foreach (var model in await ModelSearchAsync(specification, cancellationToken))
        {
            results.Add(GetSession(model));
        }

        return results;
    }

    private static Session GetSession(Models.Session model)
    {
        var session = new Session(model.Id, model.Token, model.IdentityId, model.IdentityName, model.DateRegistered, model.ExpiryDate);

        if (model.TenantId.HasValue)
        {
            session.WithTenantId(model.TenantId.Value);
        }

        foreach (var sessionPermission in model.SessionPermissions)
        {
            session.AddPermission(new(sessionPermission.PermissionId, sessionPermission.Permission.Name));
        }

        return session;
    }

    public async ValueTask<int> RemoveAsync(SessionSpecification specification, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(specification);

        var hasCriteria =
            specification.TenantId.HasValue ||
            specification.IdentityId.HasValue ||
            specification.Token != null ||
            !string.IsNullOrWhiteSpace(specification.IdentityName);

        if (!hasCriteria)
        {
            return await _accessDbContext.Database.ExecuteSqlAsync($"DELETE FROM [access].[Session]", cancellationToken);
        }

        _accessDbContext.Sessions.RemoveRange(await ModelSearchAsync(specification, cancellationToken));

        return await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(Session session, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(session);

        var model = await _accessDbContext.Sessions
            .Include(item => item.SessionPermissions)
            .SingleOrDefaultAsync(item => item.IdentityName == session.IdentityName, cancellationToken);

        if (model == null)
        {
            _accessDbContext.Sessions.Add(new()
            {
                Id = session.Id,
                IdentityName = session.IdentityName,
                DateRegistered = session.DateRegistered,
                ExpiryDate = session.ExpiryDate,
                IdentityId = session.IdentityId,
                TenantId = session.TenantId,
                Token = session.Token,
                SessionPermissions = session.Permissions.Select(p => new SessionPermission
                {
                    SessionId = session.Id,
                    PermissionId = p.Id
                }).ToList()
            });
        }
        else
        {
            model.Token = session.Token;
            model.ExpiryDate = session.ExpiryDate;
            model.TenantId = session.TenantId;

            _accessDbContext.SessionPermissions.RemoveRange(model.SessionPermissions);

            model.SessionPermissions.Clear();

            await _accessDbContext.SaveChangesAsync(cancellationToken);

            model.SessionPermissions = session.Permissions.Select(p => new SessionPermission
            {
                SessionId = session.Id,
                PermissionId = p.Id
            }).ToList();
        }

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Session> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return GetSession((await _accessDbContext.Sessions.FirstOrDefaultAsync(e => e.Id == id, cancellationToken: cancellationToken)).GuardAgainstRecordNotFound(id));
    }

    private async Task<IEnumerable<Models.Session>> ModelSearchAsync(SessionSpecification specification, CancellationToken cancellationToken = default)
    {
        var queryable = _accessDbContext.Sessions
            .Include(item => item.Tenant)
            .Include(e => e.SessionPermissions)
            .ThenInclude(e => e.Permission)
            .AsNoTracking()
            .AsQueryable();

        if (specification.HasIds)
        {
            queryable = queryable.Where(e => specification.Ids.Contains(e.Id));
        }

        if (specification.Token != null)
        {
            queryable = queryable.Where(e => e.Token == specification.Token);
        }

        if (!string.IsNullOrWhiteSpace(specification.IdentityName))
        {
            queryable = queryable.Where(e => e.IdentityName == specification.IdentityName);
        }

        if (specification.TenantId.HasValue)
        {
            queryable = queryable.Where(e => e.TenantId == specification.TenantId.Value);
        }

        if (specification.IdentityId.HasValue)
        {
            queryable = queryable.Where(e => e.IdentityId == specification.IdentityId.Value);
        }

        return await queryable.ToListAsync(cancellationToken);
    }
}