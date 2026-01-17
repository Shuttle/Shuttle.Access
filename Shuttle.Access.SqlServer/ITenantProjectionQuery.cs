using Shuttle.Access.Events.Tenant.v1;
using Shuttle.Recall;

namespace Shuttle.Access.SqlServer;

public interface ITenantProjectionQuery
{
    Task RegisteredAsync(PrimitiveEvent primitiveEvent, Registered domainEvent, CancellationToken cancellationToken = default);
}