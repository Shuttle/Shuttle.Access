using Microsoft.Extensions.Logging;
using Shuttle.Access.SqlServer;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.EventHandlers;

public class IdentityHandler(ILogger<IdentityHandler> logger, IIdentityProjectionQuery query, ISessionRepository sessionRepository)
    :
        IEventHandler<Registered>,
        IEventHandler<RoleAdded>,
        IEventHandler<RoleRemoved>,
        IEventHandler<Removed>,
        IEventHandler<Activated>,
        IEventHandler<NameSet>,
        IEventHandler<DescriptionSet>
{
    private readonly IIdentityProjectionQuery _identityProjectionQuery = Guard.AgainstNull(query);
    private readonly ILogger<IdentityHandler> _logger = Guard.AgainstNull(logger);
    private readonly ISessionRepository _sessionRepository = Guard.AgainstNull(sessionRepository);

    public async Task ProcessEventAsync(IEventHandlerContext<Activated> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _identityProjectionQuery.ActivatedAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[Activated] : id = '{context.PrimitiveEvent.Id}' / date activated = '{context.Event.DateActivated:O}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<DescriptionSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _identityProjectionQuery.DescriptionSetAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[NameSet] : id = '{context.PrimitiveEvent.Id}' / description = '{context.Event.Description}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<NameSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _identityProjectionQuery.NameSetAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[NameSet] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Registered> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _identityProjectionQuery.RegisterAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[Registered] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}' / activated = '{context.Event.Activated}' / date registered = '{context.Event.DateRegistered}' / registered by = '{context.Event.RegisteredBy}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Removed> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _sessionRepository.RemoveAsync(context.PrimitiveEvent.Id, cancellationToken);
        await _identityProjectionQuery.RemovedAsync(context.PrimitiveEvent, cancellationToken);

        _logger.LogDebug($"[Removed] : id = '{context.PrimitiveEvent.Id}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<RoleAdded> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _identityProjectionQuery.RoleAddedAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[RoleAdded] : id = '{context.PrimitiveEvent.Id}' / role id = '{context.Event.RoleId}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<RoleRemoved> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _identityProjectionQuery.RoleRemovedAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[RoleRemoved] : id = '{context.PrimitiveEvent.Id}' / role id = '{context.Event.RoleId}'");
    }
}