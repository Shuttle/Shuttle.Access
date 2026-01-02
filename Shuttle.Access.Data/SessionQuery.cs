using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Data;

public class SessionQuery(AccessDbContext accessDbContext) : ISessionQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<int> CountAsync(Models.Session.Specification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification).CountAsync(cancellationToken);
    }

    public async ValueTask<bool> ContainsAsync(Models.Session.Specification specification, CancellationToken cancellationToken = default)
    {
        return await CountAsync(specification, cancellationToken) > 0;
    }

    public async Task<IEnumerable<Models.Session>> SearchAsync(Models.Session.Specification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification)
            .OrderBy(e => e.IdentityName)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Models.Session> GetQueryable(Models.Session.Specification specification)
    {
        var queryable = _accessDbContext.Sessions.AsQueryable();

        if (specification.Token != null)
        {
            queryable = queryable.Where(e => e.Token == specification.Token);
        }

        if (specification.IdentityId.HasValue)
        {
            queryable = queryable.Where(e => e.IdentityId == specification.IdentityId);
        }

        if (!string.IsNullOrWhiteSpace((specification.IdentityName)))
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