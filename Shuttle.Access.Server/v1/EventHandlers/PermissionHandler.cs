using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shuttle.Access.Data;
using Shuttle.Access.Events.Permission.v1;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.EventHandlers;

public class PermissionHandler(ILogger<PermissionHandler> logger, IPermissionProjectionQuery query)
    :
        IEventHandler<Registered>,
        IEventHandler<Activated>,
        IEventHandler<Deactivated>,
        IEventHandler<Removed>,
        IEventHandler<NameSet>,
        IEventHandler<DescriptionSet>
{
    private readonly ILogger<PermissionHandler> _logger = Guard.AgainstNull(logger);
    private readonly IPermissionProjectionQuery _query = Guard.AgainstNull(query);

    public async Task ProcessEventAsync(IEventHandlerContext<Activated> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _query.ActivatedAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[Activated] : id = '{context.PrimitiveEvent.Id}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Deactivated> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _query.DeactivatedAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[Deactivated] : id = '{context.PrimitiveEvent.Id}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<DescriptionSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _query.DescriptionSetAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[NameSet] : id = '{context.PrimitiveEvent.Id}' / description = '{context.Event.Description}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<NameSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _query.NameSetAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[NameSet] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Registered> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _query.RegisteredAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[Registered] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}' / status = '{context.Event.Status}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Removed> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _query.RemovedAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[Removed] : id = '{context.PrimitiveEvent.Id}'");
    }
}