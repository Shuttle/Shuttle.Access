using Shuttle.Access.Events.Role.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Projection.Handlers
{
    public class RoleHandler :
        IEventHandler<Added>,
        IEventHandler<Removed>,
        IEventHandler<PermissionAdded>,
        IEventHandler<PermissionRemoved>
    {
        private readonly IRoleProjectionQuery _query;

        public RoleHandler(IRoleProjectionQuery query)
        {
            Guard.AgainstNull(query, nameof(query));

            _query = query;
        }

        public void ProcessEvent(IEventHandlerContext<Added> context)
        {
            _query.Added(context.PrimitiveEvent, context.Event);
        }

        public void ProcessEvent(IEventHandlerContext<PermissionAdded> context)
        {
            _query.PermissionAdded(context.PrimitiveEvent, context.Event);
        }

        public void ProcessEvent(IEventHandlerContext<PermissionRemoved> context)
        {
            _query.PermissionRemoved(context.PrimitiveEvent, context.Event);
        }

        public void ProcessEvent(IEventHandlerContext<Removed> context)
        {
            _query.Removed(context.PrimitiveEvent, context.Event);
        }
    }
}