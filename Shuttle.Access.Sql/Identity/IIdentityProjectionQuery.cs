﻿using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Recall;

namespace Shuttle.Access.Sql;

public interface IIdentityProjectionQuery
{
    Task ActivatedAsync(PrimitiveEvent primitiveEvent, Activated domainEvent, CancellationToken cancellationToken = default);
    Task NameSetAsync(PrimitiveEvent primitiveEvent, NameSet domainEvent, CancellationToken cancellationToken = default);
    Task RegisterAsync(PrimitiveEvent primitiveEvent, Registered domainEvent, CancellationToken cancellationToken = default);
    Task RemovedAsync(PrimitiveEvent primitiveEvent, CancellationToken cancellationToken = default);
    Task RoleAddedAsync(PrimitiveEvent primitiveEvent, RoleAdded domainEvent, CancellationToken cancellationToken = default);
    Task RoleRemovedAsync(PrimitiveEvent primitiveEvent, RoleRemoved domainEvent, CancellationToken cancellationToken = default);
}