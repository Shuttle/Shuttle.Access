using System;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application
{
    public class AddRoleParticipant : IParticipant<RequestResponseMessage<AddRole, RoleAdded>>
    {
        private readonly IEventStore _eventStore;
        private readonly IKeyStore _keyStore;

        public AddRoleParticipant(IEventStore eventStore, IKeyStore keyStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(keyStore, nameof(keyStore));

            _eventStore = eventStore;
            _keyStore = keyStore;
        }

        public void ProcessMessage(IParticipantContext<RequestResponseMessage<AddRole, RoleAdded>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message.Request;

            var key = Role.Key(message.Name);

            if (_keyStore.Contains(key))
            {
                return;
            }

            var id = Guid.NewGuid();

            _keyStore.Add(id, key);

            var role = new Role(id);
            var stream = _eventStore.CreateEventStream(id);

            stream.AddEvent(role.Add(message.Name));

            _eventStore.Save(stream);

            context.Message.WithResponse(new RoleAdded
            {
                Id = id,
                Name = message.Name
            });
        }
    }
}