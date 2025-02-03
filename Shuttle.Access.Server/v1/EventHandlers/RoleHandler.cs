using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.EventHandlers;

public class RoleHandler :
    IEventHandler<Registered>,
    IEventHandler<Removed>,
    IEventHandler<PermissionAdded>,
    IEventHandler<PermissionRemoved>,
    IEventHandler<NameSet>
{
    private readonly ILogger<RoleHandler> _logger;
    private readonly IRoleProjectionQuery _query;

    public RoleHandler(ILogger<RoleHandler> logger, IRoleProjectionQuery query)
    {
        _logger = Guard.AgainstNull(logger);
        _query = Guard.AgainstNull(query);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<NameSet> context)
    {
        Guard.AgainstNull(context);

        await _query.NameSetAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[NameSet] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<PermissionAdded> context)
    {
        Guard.AgainstNull(context);

        await _query.PermissionAddedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[PermissionAdded] : id = '{context.PrimitiveEvent.Id}' / permission id = '{context.Event.PermissionId}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<PermissionRemoved> context)
    {
        Guard.AgainstNull(context);

        await _query.PermissionRemovedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[PermissionRemoved] : id = '{context.PrimitiveEvent.Id}' / permission id = '{context.Event.PermissionId}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Registered> context)
    {
        Guard.AgainstNull(context);

        await _query.RegisteredAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[Registered] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Removed> context)
    {
        Guard.AgainstNull(context);

        await _query.RemovedAsync(context.PrimitiveEvent, context.CancellationToken);

        _logger.LogDebug($"[Removed] : id = '{context.PrimitiveEvent.Id}'");
    }
}