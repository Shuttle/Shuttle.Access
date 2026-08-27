using Microsoft.EntityFrameworkCore;
using Shuttle.Contract;

namespace Shuttle.Access.SqlServer;

public class AttributeDefinitionQuery(AccessDbContext accessDbContext) : IAttributeDefinitionQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async ValueTask<int> CountAsync(Query.AttributeDefinition.Specification specification, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(specification).CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Query.AttributeDefinition>> SearchAsync(Query.AttributeDefinition.Specification specification, CancellationToken cancellationToken = default)
    {
        return (await GetQueryable(specification)
                .OrderBy(e => e.Name)
                .ToListAsync(cancellationToken))
            .Select(e => new Query.AttributeDefinition
            {
                Id = e.Id,
                TenantId = e.TenantId,
                Name = e.Name,
                Description = e.Description,
                Type = (AttributeType)e.Type,
                Cardinality = (AttributeCardinality)e.Cardinality
            });
    }

    private IQueryable<Models.AttributeDefinition> GetQueryable(Query.AttributeDefinition.Specification specification)
    {
        var queryable = _accessDbContext.AttributeDefinitions.AsNoTracking().AsQueryable();

        if (specification.TenantId.HasValue)
        {
            queryable = queryable.Where(e => e.TenantId == specification.TenantId.Value);
        }

        if (!string.IsNullOrWhiteSpace(specification.NameMatch))
        {
            queryable = queryable.Where(e => EF.Functions.Like(e.Name, $"%{specification.NameMatch}%"));
        }

        if (specification.Names.Any())
        {
            queryable = queryable.Where(e => specification.Names.Contains(e.Name));
        }

        if (specification.HasIds)
        {
            queryable = queryable.Where(e => specification.Ids.Contains(e.Id));
        }

        if (specification.MaximumRows > 0)
        {
            queryable = queryable.Take(specification.MaximumRows);
        }

        return queryable;
    }
}
