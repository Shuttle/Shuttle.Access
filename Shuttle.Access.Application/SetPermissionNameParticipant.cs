using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application
{
    public class SetPermissionNameParticipant : IParticipant<RequestResponseMessage<SetPermissionName, PermissionNameSet>>
    {
        private readonly IEventStore _eventStore;
        private readonly IKeyStore _keyStore;

        public SetPermissionNameParticipant(IEventStore eventStore, IKeyStore keyStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(keyStore, nameof(keyStore));

            _eventStore = eventStore;
            _keyStore = keyStore;
        }

        public void ProcessMessage(IParticipantContext<RequestResponseMessage<SetPermissionName, PermissionNameSet>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var request = context.Message.Request;
            var id = request.Id;

            var permission = new Permission();
            var stream = _eventStore.Get(request.Id);

            stream.Apply(permission);

            if (permission.Name.Equals(request.Name))
            {
                return;
            }

            var key = Permission.Key(permission.Name);
            var rekey = Permission.Key(request.Name);

            if (_keyStore.Contains(rekey) || !_keyStore.Contains(key))
            {
                return;
            }

            _keyStore.Rekey(key, rekey);

            stream.AddEvent(permission.SetName(request.Name));

            context.Message.WithResponse(new PermissionNameSet
            {
                Id = request.Id,
                Name = request.Name,
                SequenceNumber = _eventStore.Save(stream)
            });
        }
    }
}