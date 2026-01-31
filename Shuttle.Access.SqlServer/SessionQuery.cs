using Microsoft.EntityFrameworkCore;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class SessionQuery(AccessDbContext accessDbContext) : ISessionQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<int> CountAsync(SessionSpecification sessionSpecification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(sessionSpecification).CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Models.Session>> SearchAsync(SessionSpecification sessionSpecification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(sessionSpecification)
            .Include(e => e.Identity)
            .Include(e => e.SessionPermissions)
            .ThenInclude(e => e.Permission)
            .OrderBy(e => e.IdentityName)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Models.Session> GetQueryable(SessionSpecification sessionSpecification)
    {
        var queryable = _accessDbContext.Sessions.AsQueryable();

        if (sessionSpecification.Token != null)
        {
            queryable = queryable.Where(e => e.Token == sessionSpecification.Token);
        }

        if (sessionSpecification.IdentityId.HasValue)
        {
            queryable = queryable.Where(e => e.IdentityId == sessionSpecification.IdentityId);
        }

        if (!string.IsNullOrWhiteSpace(sessionSpecification.IdentityName))
        {
            queryable = queryable.Where(e => e.IdentityName == sessionSpecification.IdentityName);
        }

        if (!string.IsNullOrWhiteSpace(sessionSpecification.IdentityNameMatch))
        {
            queryable = queryable.Where(e => e.IdentityName.Contains(sessionSpecification.IdentityNameMatch));
        }

        if (sessionSpecification.Permissions.Any())
        {
            queryable = queryable.Where(e => e.SessionPermissions.Any(p => sessionSpecification.Permissions.Contains(p.Permission.Name)));
        }

        return queryable;
    }
}