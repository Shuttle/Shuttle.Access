using System.Threading.Tasks;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Projection.v1;

public class IdentityHandler :
    IAsyncEventHandler<Registered>,
    IAsyncEventHandler<RoleAdded>,
    IAsyncEventHandler<RoleRemoved>,
    IAsyncEventHandler<Removed>,
    IAsyncEventHandler<Activated>,
    IAsyncEventHandler<NameSet>
{
    private readonly IIdentityProjectionQuery _query;

    public IdentityHandler(IIdentityProjectionQuery query)
    {
        _query = Guard.AgainstNull(query);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Activated> context)
    {
        Guard.AgainstNull(context);

        await _query.ActivatedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<NameSet> context)
    {
        Guard.AgainstNull(context);

        await _query.NameSetAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Registered> context)
    {
        Guard.AgainstNull(context);

        await _query.RegisterAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Removed> context)
    {
        Guard.AgainstNull(context);

        await _query.RemovedAsync(context.PrimitiveEvent, context.CancellationToken);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<RoleAdded> context)
    {
        Guard.AgainstNull(context);

        await _query.RoleAddedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<RoleRemoved> context)
    {
        Guard.AgainstNull(context);

        await _query.RoleRemovedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);
    }
}