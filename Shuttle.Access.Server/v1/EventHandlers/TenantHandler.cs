using Microsoft.Extensions.Logging;
using Shuttle.Access.Events.Tenant.v1;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Contract;
using Shuttle.Recall;
namespace Shuttle.Access.Server.v1.EventHandlers;

public class TenantHandler(ILogger<TenantHandler> logger, ITenantProjectionQuery query) : 
    IEventHandler<Registered>,
    IEventHandler<StatusSet>
{
    private readonly ILogger<TenantHandler> _logger = Guard.AgainstNull(logger);
    private readonly ITenantProjectionQuery _query = Guard.AgainstNull(query);

    public async Task HandleAsync(IEventHandlerContext<Registered> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _query.RegisteredAsync(context.PrimitiveEvent, context.Event, cancellationToken);

        _logger.LogDebug($"[Registered] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}'");
    }

    public async Task HandleAsync(IEventHandlerContext<StatusSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _query.StatusSetAsync(context.PrimitiveEvent, context.Event, cancellationToken);
    }
}