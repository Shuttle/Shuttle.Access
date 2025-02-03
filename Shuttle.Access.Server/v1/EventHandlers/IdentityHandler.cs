using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.EventHandlers;

public class IdentityHandler :
    IEventHandler<Registered>,
    IEventHandler<RoleAdded>,
    IEventHandler<RoleRemoved>,
    IEventHandler<Removed>,
    IEventHandler<Activated>,
    IEventHandler<NameSet>
{
    private readonly ILogger<IdentityHandler> _logger;
    private readonly IIdentityProjectionQuery _query;

    public IdentityHandler(ILogger<IdentityHandler> logger, IIdentityProjectionQuery query)
    {
        _logger = Guard.AgainstNull(logger);
        _query = Guard.AgainstNull(query);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Activated> context)
    {
        Guard.AgainstNull(context);

        await _query.ActivatedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[Activated] : id = '{context.PrimitiveEvent.Id}' / date activated = '{context.Event.DateActivated:O}'");
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

        await _query.RegisterAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[Registered] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}' / activated = '{context.Event.Activated}' / date registered = '{context.Event.DateRegistered}' / registered by = '{context.Event.RegisteredBy}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Removed> context)
    {
        Guard.AgainstNull(context);

        await _query.RemovedAsync(context.PrimitiveEvent, context.CancellationToken);

        _logger.LogDebug($"[Removed] : id = '{context.PrimitiveEvent.Id}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<RoleAdded> context)
    {
        Guard.AgainstNull(context);

        await _query.RoleAddedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[RoleAdded] : id = '{context.PrimitiveEvent.Id}' / role id = '{context.Event.RoleId}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<RoleRemoved> context)
    {
        Guard.AgainstNull(context);

        await _query.RoleRemovedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[RoleRemoved] : id = '{context.PrimitiveEvent.Id}' / role id = '{context.Event.RoleId}'");
    }
}