using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.Events.Permission.v1;
using Shuttle.Recall;

namespace Shuttle.Access.Sql;

public interface IPermissionProjectionQuery
{
    Task ActivatedAsync(PrimitiveEvent primitiveEvent, Activated domainEvent, CancellationToken cancellationToken = default);
    Task DeactivatedAsync(PrimitiveEvent primitiveEvent, Deactivated domainEvent, CancellationToken cancellationToken = default);
    Task NameSetAsync(PrimitiveEvent primitiveEvent, NameSet domainEvent, CancellationToken cancellationToken = default);
    Task RegisteredAsync(PrimitiveEvent primitiveEvent, Registered domainEvent, CancellationToken cancellationToken = default);
    Task RemovedAsync(PrimitiveEvent primitiveEvent, Removed domainEvent, CancellationToken cancellationToken = default);
}