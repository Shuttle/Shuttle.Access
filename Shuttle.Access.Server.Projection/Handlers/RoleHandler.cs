using Shuttle.Core.Infrastructure;
using Shuttle.Recall;
using Shuttle.Sentinel.DomainEvents.Role.v1;

namespace Shuttle.Sentinel.Server.Projection
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