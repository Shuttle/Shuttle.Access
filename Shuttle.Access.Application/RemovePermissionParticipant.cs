using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class RemovePermissionParticipant : IParticipant<RequestResponseMessage<RemovePermission, PermissionRemoved>>
    {
        private readonly IEventStore _eventStore;

        public RemovePermissionParticipant(IEventStore eventStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));

            _eventStore = eventStore;
        }

        public void ProcessMessage(
            IParticipantContext<RequestResponseMessage<RemovePermission, PermissionRemoved>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            var stream = _eventStore.Get(message.Request.Id);

            if (stream.IsEmpty)
            {
                return;
            }

            var permission = new Permission();

            stream.Apply(permission);

            stream.AddEvent(permission.Remove());

            _eventStore.Save(stream);

            context.Message.WithResponse(new PermissionRemoved
            {
                Id = message.Request.Id,
                Name = permission.Name
            });
        }
    }
}