using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class SetRolePermissionParticipant : IParticipant<RequestResponseMessage<SetRolePermission, RolePermissionSet>>
    {
        private readonly IEventStore _eventStore;

        public SetRolePermissionParticipant(IEventStore eventStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));

            _eventStore = eventStore;
        }

        public void ProcessMessage(IParticipantContext<RequestResponseMessage<SetRolePermission, RolePermissionSet>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;
            var request = message.Request;

            var role = new Role();
            var stream = _eventStore.Get(request.RoleId);

            stream.Apply(role);

            if (request.Active && !role.HasPermission(request.PermissionId))
            {
                stream.AddEvent(role.AddPermission(request.PermissionId));
            }

            if (!request.Active && role.HasPermission(request.PermissionId))
            {
                stream.AddEvent(role.RemovePermission(request.PermissionId));
            }

            _eventStore.Save(stream);

            message.WithResponse(new RolePermissionSet
            {
                RoleId = request.RoleId,
                PermissionId = request.PermissionId,
                Active = request.Active
            });
        }
    }
}