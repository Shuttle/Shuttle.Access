using Microsoft.EntityFrameworkCore;
using Shuttle.Access.Events.Tenant.v1;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.SqlServer;

public class TenantProjectionQuery(AccessDbContext accessDbContext) : ITenantProjectionQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async Task RegisteredAsync(PrimitiveEvent primitiveEvent, Registered domainEvent, CancellationToken cancellationToken = default)
    {
        var model = await _accessDbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(item => item.Id == primitiveEvent.Id, cancellationToken: cancellationToken);

        if (model != null)
        {
            return;
        }

        _accessDbContext.Tenants.Add(new()
        {
            Id = primitiveEvent.Id,
            Name = domainEvent.Name
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }
}