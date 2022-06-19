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

            var role = new Role();
            var stream = _eventStore.Get(request.Id);

            stream.Apply(role);

            if (role.Name.Equals(request.Name))
            {
                return;
            }

            var key = Role.Key(role.Name);
            var rekey = Role.Key(request.Name);

            if (_keyStore.Contains(rekey) || !_keyStore.Contains(key))
            {
                return;
            }

            _keyStore.Rekey(key, rekey);

            stream.AddEvent(role.SetName(request.Name));

            context.Message.WithResponse(new RoleNameSet
            {
                Id = request.Id,
                Name = request.Name,
                SequenceNumber = _eventStore.Save(stream)
            });
        }
    }
}