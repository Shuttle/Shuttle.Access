using Shuttle.Access.Events.Role.v1;
using Shuttle.Recall;

namespace Shuttle.Access.SqlServer;

public interface IRoleProjectionQuery
{
    Task NameSetAsync(PrimitiveEvent primitiveEvent, NameSet domainEvent, CancellationToken cancellationToken = default);
    Task PermissionAddedAsync(PrimitiveEvent primitiveEvent, PermissionAdded domainEvent, CancellationToken cancellationToken = default);
    Task PermissionRemovedAsync(PrimitiveEvent primitiveEvent, PermissionRemoved domainEvent, CancellationToken cancellationToken = default);
    Task RegisteredAsync(PrimitiveEvent primitiveEvent, Registered domainEvent, CancellationToken cancellationToken = default);
    Task RemovedAsync(PrimitiveEvent primitiveEvent, CancellationToken cancellationToken = default);
}