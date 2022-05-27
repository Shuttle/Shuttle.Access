using Shuttle.Access.Events.Permission.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Projection.Handlers
{
    public class PermissionHandler :
        IEventHandler<Registered>,
        IEventHandler<Activated>,
        IEventHandler<Deactivated>,
        IEventHandler<Removed>,
        IEventHandler<NameSet>
    {
        private readonly IPermissionProjectionQuery _query;

        public PermissionHandler(IPermissionProjectionQuery query)
        {
            Guard.AgainstNull(query, nameof(query));

            _query = query;
        }

        public void ProcessEvent(IEventHandlerContext<Registered> context)
        {
            Guard.AgainstNull(context, nameof(context));

            _query.Registered(context.PrimitiveEvent, context.Event);
        }

        public void ProcessEvent(IEventHandlerContext<Activated> context)
        {
            Guard.AgainstNull(context, nameof(context));

            _query.Activated(context.PrimitiveEvent, context.Event);
        }

        public void ProcessEvent(IEventHandlerContext<Deactivated> context)
        {
            Guard.AgainstNull(context, nameof(context));

            _query.Deactivated(context.PrimitiveEvent, context.Event);
        }

        public void ProcessEvent(IEventHandlerContext<Removed> context)
        {
            Guard.AgainstNull(context, nameof(context));

            _query.Removed(context.PrimitiveEvent, context.Event);
        }

        public void ProcessEvent(IEventHandlerContext<NameSet> context)
        {
            Guard.AgainstNull(context, nameof(context));

            _query.NameSet(context.PrimitiveEvent, context.Event);
        }
    }
}