using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Infrastructure;
using Shuttle.Recall;

namespace Shuttle.Access.Server.Projection.Handlers
{
    public class RoleHandler :
        IEventHandler<Added>,
        IEventHandler<Removed>,
        IEventHandler<PermissionAdded>,
        IEventHandler<PermissionRemoved>
    {
        private readonly ISystemRoleQuery _query;

        public RoleHandler(ISystemRoleQuery query)
        {
            Guard.AgainstNull(query, "query");

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