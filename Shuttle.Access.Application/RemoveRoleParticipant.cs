using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class RemoveRoleParticipant : IParticipant<RequestResponseMessage<RemoveRole, RoleRemoved>>
    {
        private readonly IEventStore _eventStore;

        public RemoveRoleParticipant(IEventStore eventStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));

            _eventStore = eventStore;
        }

        public void ProcessMessage(IParticipantContext<RequestResponseMessage<RemoveRole, RoleRemoved>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            var stream = _eventStore.Get(message.Request.Id);

            if (stream.IsEmpty)
            {
                return;
            }

            var role = new Role();

            stream.Apply(role);

            stream.AddEvent(role.Remove());

            _eventStore.Save(stream);

            context.Message.WithResponse(new RoleRemoved
            {
                Id = message.Request.Id,                            
                Name = role.Name
            });
        }
    }
}