using Shuttle.Access.Events.User.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Projection.Handlers
{
    public class UserHandler :
        IEventHandler<Registered>,
        IEventHandler<RoleAdded>,
        IEventHandler<RoleRemoved>,
        IEventHandler<Removed>,
        IEventHandler<Activated>
    {
        private readonly ISystemUserProjectionQuery _query;

        public UserHandler(ISystemUserProjectionQuery query)
        {
            Guard.AgainstNull(query, nameof(query));

            _query = query;
        }

        public void ProcessEvent(IEventHandlerContext<Registered> context)
        {
            Guard.AgainstNull(context, nameof(context));
            
            _query.Register(context.PrimitiveEvent, context.Event);
        }

        public void ProcessEvent(IEventHandlerContext<RoleAdded> context)
        {
            Guard.AgainstNull(context, nameof(context));

            _query.RoleAdded(context.PrimitiveEvent, context.Event);
        }

        public void ProcessEvent(IEventHandlerContext<RoleRemoved> context)
        {
            Guard.AgainstNull(context, nameof(context));

            _query.RoleRemoved(context.PrimitiveEvent, context.Event);
        }

        public void ProcessEvent(IEventHandlerContext<Removed> context)
        {
            Guard.AgainstNull(context, nameof(context));

            _query.Removed(context.PrimitiveEvent, context.Event);
        }

        public void ProcessEvent(IEventHandlerContext<Activated> context)
        {
            Guard.AgainstNull(context, nameof(context));

            _query.Activated(context.PrimitiveEvent, context.Event);
        }
    }
}