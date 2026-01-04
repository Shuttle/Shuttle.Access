using Microsoft.EntityFrameworkCore;
using Shuttle.Access.SqlServer.Models;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class SessionRepository(AccessDbContext accessDbContext) : ISessionRepository
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async Task RemoveAllAsync(CancellationToken cancellationToken = default)
    {
        await _accessDbContext.Database.ExecuteSqlAsync($"DELETE FROM [access].[Session]", cancellationToken: cancellationToken);
    }

    public async Task SaveAsync(Session session, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(session);

        var model = await _accessDbContext.Sessions.SingleOrDefaultAsync(e => e.IdentityName == session.IdentityName, cancellationToken: cancellationToken);

        if (model == null)
        {
            _accessDbContext.Sessions.Add(new()
            {
                IdentityName = session.IdentityName,
                DateRegistered = session.DateRegistered,
                ExpiryDate = session.ExpiryDate,
                IdentityId = session.IdentityId,
                Token = session.Token,
                SessionPermissions = session.Permissions.Select(p => new SessionPermission
                {
                    IdentityId = session.IdentityId,
                    PermissionId = p.Id
                }).ToList()
            });
        }
        else
        {
            model.IdentityId = session.IdentityId;
            model.Token = session.Token;
            model.ExpiryDate = session.ExpiryDate;
            model.SessionPermissions = session.Permissions.Select(p => new SessionPermission
            {
                IdentityId = session.IdentityId,
                PermissionId = p.Id
            }).ToList();
        }

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Session?> FindAsync(byte[] token, CancellationToken cancellationToken = default)
    {

        return await FindAsync(new SqlServer.Models.Session.Specification().WithToken(token), cancellationToken);
    }

    public async Task<Session?> FindAsync(string identityName, CancellationToken cancellationToken = default)
    {
        return await FindAsync(new SqlServer.Models.Session.Specification().WithIdentityName(identityName), cancellationToken);
    }

    public async Task<Session?> FindAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        return await FindAsync(new SqlServer.Models.Session.Specification().WithIdentityId(identityId), cancellationToken);
    }

    private async Task<Session?> FindAsync(SqlServer.Models.Session.Specification specification, CancellationToken cancellationToken)
    {
        var queryable = _accessDbContext.Sessions
            .Include(e => e.SessionPermissions)
            .ThenInclude(e => e.Permission)
            .AsNoTracking().AsQueryable();

        if (specification.Token != null)
        {
            queryable = queryable.Where(e => e.Token == specification.Token);
        }

        if (!string.IsNullOrWhiteSpace(specification.IdentityName))
        {
            queryable = queryable.Where(e => e.IdentityName == specification.IdentityName);
        }

        if (specification.IdentityId.HasValue)
        {
            queryable = queryable.Where(e => e.IdentityId == specification.IdentityId.Value);
        }

        var model = await queryable.SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (model == null)
        {
            return null;
        }

        var result = new Session(model.Token, model.IdentityId, model.IdentityName, model.DateRegistered, model.ExpiryDate);

        foreach (var sessionPermission in model.SessionPermissions)
        {
            result.AddPermission(new(sessionPermission.PermissionId, sessionPermission.Permission.Name));
        }

        return result;
    }

    public async ValueTask<bool> RemoveAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        var model = await _accessDbContext.Sessions.SingleOrDefaultAsync(e => e.IdentityId == identityId, cancellationToken: cancellationToken);

        if (model == null)
        {
            return false;
        }

        _accessDbContext.Sessions.Remove(model);

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}