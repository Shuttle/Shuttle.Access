using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Sql;
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
    private readonly ILogger<IdentityHandler> _logger = Guard.AgainstNull(logger);
    private readonly IIdentityProjectionQuery _identityProjectionQuery = Guard.AgainstNull(query);
    private readonly ISessionRepository _sessionRepository = Guard.AgainstNull(sessionRepository);

    public async Task ProcessEventAsync(IEventHandlerContext<Activated> context)
    {
        Guard.AgainstNull(context);

        await _identityProjectionQuery.ActivatedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[Activated] : id = '{context.PrimitiveEvent.Id}' / date activated = '{context.Event.DateActivated:O}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<DescriptionSet> context)
    {
        Guard.AgainstNull(context);

        await _identityProjectionQuery.DescriptionSetAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[NameSet] : id = '{context.PrimitiveEvent.Id}' / description = '{context.Event.Description}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<NameSet> context)
    {
        Guard.AgainstNull(context);

        await _identityProjectionQuery.NameSetAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[NameSet] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Registered> context)
    {
        Guard.AgainstNull(context);

        await _identityProjectionQuery.RegisterAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[Registered] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}' / activated = '{context.Event.Activated}' / date registered = '{context.Event.DateRegistered}' / registered by = '{context.Event.RegisteredBy}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Removed> context)
    {
        Guard.AgainstNull(context);

        await _sessionRepository.RemoveAsync(context.PrimitiveEvent.Id, context.CancellationToken);
        await _identityProjectionQuery.RemovedAsync(context.PrimitiveEvent, context.CancellationToken);

        _logger.LogDebug($"[Removed] : id = '{context.PrimitiveEvent.Id}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<RoleAdded> context)
    {
        Guard.AgainstNull(context);

        await _identityProjectionQuery.RoleAddedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[RoleAdded] : id = '{context.PrimitiveEvent.Id}' / role id = '{context.Event.RoleId}'");
    }

    public async Task ProcessEventAsync(IEventHandlerContext<RoleRemoved> context)
    {
        Guard.AgainstNull(context);

        await _identityProjectionQuery.RoleRemovedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);

        _logger.LogDebug($"[RoleRemoved] : id = '{context.PrimitiveEvent.Id}' / role id = '{context.Event.RoleId}'");
    }
}