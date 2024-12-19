using System.Threading.Tasks;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Projection.v1;

public class RoleHandler :
    IEventHandler<Registered>,
    IEventHandler<Removed>,
    IEventHandler<PermissionAdded>,
    IEventHandler<PermissionRemoved>,
    IEventHandler<NameSet>
{
    private readonly IRoleProjectionQuery _query;

    public RoleHandler(IRoleProjectionQuery query)
    {
        _query = Guard.AgainstNull(query);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<NameSet> context)
    {
        Guard.AgainstNull(context);

        await _query.NameSetAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<PermissionAdded> context)
    {
        Guard.AgainstNull(context);

        await _query.PermissionAddedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<PermissionRemoved> context)
    {
        Guard.AgainstNull(context);

        await _query.PermissionRemovedAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Registered> context)
    {
        Guard.AgainstNull(context);

        await _query.RegisteredAsync(context.PrimitiveEvent, context.Event, context.CancellationToken);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Removed> context)
    {
        Guard.AgainstNull(context);

        await _query.RemovedAsync(context.PrimitiveEvent, context.CancellationToken);
    }
}