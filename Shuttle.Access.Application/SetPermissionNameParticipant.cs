using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class SetPermissionNameParticipant : IParticipant<RequestResponseMessage<SetPermissionName, PermissionNameSet>>
    {
        private readonly IEventStore _eventStore;

        public SetPermissionNameParticipant(IEventStore eventStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));

            _eventStore = eventStore;
        }

        public void ProcessMessage(IParticipantContext<RequestResponseMessage<SetPermissionName, PermissionNameSet>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var request = context.Message.Request;
            var permission = new Permission();
            var stream = _eventStore.Get(request.Id);

            stream.Apply(permission);

            if (permission.Name.Equals(request.Name))
            {
                return;
            }

            stream.AddEvent(permission.SetName(request.Name));

            _eventStore.Save(stream);

            context.Message.WithResponse(new PermissionNameSet
            {
                Id = request.Id,
                Name = request.Name
            });
        }
    }
}