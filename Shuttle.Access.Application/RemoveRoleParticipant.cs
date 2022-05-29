using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application
{
    public class RemoveRoleParticipant : IParticipant<RequestResponseMessage<RemoveRole, RoleRemoved>>
    {
        private readonly IEventStore _eventStore;
        private readonly IKeyStore _keyStore;

        public RemoveRoleParticipant(IEventStore eventStore, IKeyStore keyStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(keyStore, nameof(keyStore));

            _eventStore = eventStore;
            _keyStore = keyStore;
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

            _keyStore.Remove(message.Request.Id);

            _eventStore.Save(stream);

            context.Message.WithResponse(new RoleRemoved
            {
                Id = message.Request.Id,                            
                Name = role.Name
            });
        }
    }
}