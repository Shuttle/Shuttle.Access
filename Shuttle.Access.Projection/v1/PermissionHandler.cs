using System.Threading.Tasks;
using Shuttle.Access.Events.Permission.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Projection.v1;

public class PermissionHandler :
    IAsyncEventHandler<Registered>,
    IAsyncEventHandler<Activated>,
    IAsyncEventHandler<Deactivated>,
    IAsyncEventHandler<Removed>,
    IAsyncEventHandler<NameSet>
{
    private readonly IPermissionProjectionQuery _query;

    public PermissionHandler(IPermissionProjectionQuery query)
    {
        _query = Guard.AgainstNull(query);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Activated> context)
    {
        Guard.AgainstNull(context);

        await _query.ActivatedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Deactivated> context)
    {
        Guard.AgainstNull(context);

        await _query.DeactivatedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<NameSet> context)
    {
        Guard.AgainstNull(context);

        await _query.NameSetAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Registered> context)
    {
        Guard.AgainstNull(context);

        await _query.RegisteredAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Removed> context)
    {
        Guard.AgainstNull(context);

        await _query.RemovedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);
    }
}