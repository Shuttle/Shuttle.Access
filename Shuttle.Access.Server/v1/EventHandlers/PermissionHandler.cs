using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shuttle.Access.Events.Permission.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.EventHandlers;

public class PermissionHandler :
    IEventHandler<Registered>,
    IEventHandler<Activated>,
    IEventHandler<Deactivated>,
    IEventHandler<Removed>,
    IEventHandler<NameSet>
{
    private readonly ILogger<PermissionHandler> _logger;
    private readonly IPermissionProjectionQuery _query;

    public PermissionHandler(ILogger<PermissionHandler> logger, IPermissionProjectionQuery query)
    {
        _logger = Guard.AgainstNull(logger);
        _query = Guard.AgainstNull(query);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Activated> context)
    {
        Guard.AgainstNull(context);

        await _query.ActivatedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[Activated] : id = '{context.PrimitiveEvent.Id}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Deactivated> context)
    {
        Guard.AgainstNull(context);

        await _query.DeactivatedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[Deactivated] : id = '{context.PrimitiveEvent.Id}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<NameSet> context)
    {
        Guard.AgainstNull(context);

        await _query.NameSetAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[NameSet] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Registered> context)
    {
        Guard.AgainstNull(context);

        await _query.RegisteredAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[Registered] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}' / status = '{context.Event.Status}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Removed> context)
    {
        Guard.AgainstNull(context);

        await _query.RemovedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[Removed] : id = '{context.PrimitiveEvent.Id}'");
    }
}