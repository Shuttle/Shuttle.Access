using Microsoft.EntityFrameworkCore;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class TenantQuery(AccessDbContext accessDbContext) : ITenantQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<int> CountAsync(TenantSpecification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification).CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Models.Tenant>> SearchAsync(TenantSpecification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification)
            .OrderBy(e => e.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Models.Tenant> GetQueryable(TenantSpecification specification)
    {
        var queryable = _accessDbContext.Tenants
            .AsNoTracking()
            .AsQueryable();

        if (specification.Names.Any())
        {
            queryable = queryable.Where(e => specification.Names.Contains(e.Name));
        }

        if (!string.IsNullOrEmpty(specification.NameMatch))
        {
            queryable = queryable.Where(e => EF.Functions.Like(e.Name, $"%{specification.NameMatch}%"));
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