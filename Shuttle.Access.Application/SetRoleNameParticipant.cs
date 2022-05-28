using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application
{
    public class SetRoleNameParticipant : IParticipant<RequestResponseMessage<SetRoleName, RoleNameSet>>
    {
        private readonly IEventStore _eventStore;
        private readonly IKeyStore _keyStore;

        public SetRoleNameParticipant(IEventStore eventStore, IKeyStore keyStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(keyStore, nameof(keyStore));

            _eventStore = eventStore;
            _keyStore = keyStore;
        }

        public void ProcessMessage(IParticipantContext<RequestResponseMessage<SetRoleName, RoleNameSet>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var request = context.Message.Request;
            var key = Role.Key(request.Name);
            var id = request.Id;

            if (_keyStore.Contains(key) || !_keyStore.Contains(id))
            {
                return;
            }

            _keyStore.Rekey(id, key);

            var role = new Role();
            var stream = _eventStore.Get(id);

            stream.Apply(role);

            if (role.Name.Equals(request.Name))
            {
                return;
            }

            stream.AddEvent(role.SetName(request.Name));

            _eventStore.Save(stream);

            context.Message.WithResponse(new RoleNameSet
            {
                Id = id,
                Name = request.Name
            });
        }
    }
}