using Shuttle.Core.Infrastructure;
using Shuttle.Recall;
using Shuttle.Sentinel.DomainEvents.User.v1;

namespace Shuttle.Sentinel.Server.Projection
{
    public class UserHandler :
        IEventHandler<Registered>,
        IEventHandler<RoleAdded>,
        IEventHandler<RoleRemoved>,
        IEventHandler<Removed>
    {
        private readonly ISystemUserQuery _query;

        public UserHandler(ISystemUserQuery query)
        {
            Guard.AgainstNull(query, "query");

            _query = query;
        }

        public void ProcessEvent(IEventHandlerContext<Registered> context)
        {
            _query.Register(context.PrimitiveEvent, context.Event);
        }

        public void ProcessEvent(IEventHandlerContext<RoleAdded> context)
        {
            _query.RoleAdded(context.PrimitiveEvent, context.Event);
        }

        public void ProcessEvent(IEventHandlerContext<RoleRemoved> context)
        {
            _query.RoleRemoved(context.PrimitiveEvent, context.Event);
        }

        public void ProcessEvent(IEventHandlerContext<Removed> context)
        {
            _query.Removed(context.PrimitiveEvent, context.Event);
        }
    }
}