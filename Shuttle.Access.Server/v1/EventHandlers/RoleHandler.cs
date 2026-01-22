using Microsoft.Extensions.Logging;
using Shuttle.Access.SqlServer;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.EventHandlers;

public class RoleHandler(ILogger<RoleHandler> logger, IRoleProjectionQuery query, IPermissionQuery permissionQuery) :
    IEventHandler<Registered>,
    IEventHandler<Shuttle.Access.Events.Role.v2.Registered>,
    IEventHandler<Removed>,
    IEventHandler<PermissionAdded>,
    IEventHandler<PermissionRemoved>,
    IEventHandler<NameSet>
{
    private readonly ILogger<RoleHandler> _logger = Guard.AgainstNull(logger);
    private readonly IRoleProjectionQuery _query = Guard.AgainstNull(query);
    private readonly IPermissionQuery _permissionQuery = Guard.AgainstNull(permissionQuery);

    public async Task ProcessEventAsync(IEventHandlerContext<NameSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _query.NameSetAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[NameSet] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<PermissionAdded> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        if (!await _permissionQuery.ContainsAsync(new SqlServer.Models.Permission.Specification().AddId(context.Event.PermissionId), cancellationToken: cancellationToken))
        {
            context.Defer();
            return;
        }

        await _query.PermissionAddedAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[PermissionAdded] : id = '{context.PrimitiveEvent.Id}' / permission id = '{context.Event.PermissionId}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<PermissionRemoved> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _query.PermissionRemovedAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[PermissionRemoved] : id = '{context.PrimitiveEvent.Id}' / permission id = '{context.Event.PermissionId}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Registered> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _query.RegisteredAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[Registered] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Removed> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _query.RemovedAsync(context.PrimitiveEvent, cancellationToken);

        _logger.LogDebug($"[Removed] : id = '{context.PrimitiveEvent.Id}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Events.Role.v2.Registered> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _query.RegisteredAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[Registered] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}'");
    }
}