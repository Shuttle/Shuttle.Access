using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application
{
    public class SetIdentityNameParticipant : IParticipant<RequestResponseMessage<SetIdentityName, IdentityNameSet>>
    {
        private readonly IEventStore _eventStore;
        private readonly IKeyStore _keyStore;

        public SetIdentityNameParticipant(IEventStore eventStore, IKeyStore keyStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(keyStore, nameof(keyStore));

            _eventStore = eventStore;
            _keyStore = keyStore;
        }

        public void ProcessMessage(IParticipantContext<RequestResponseMessage<SetIdentityName, IdentityNameSet>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var request = context.Message.Request;

            var identity = new Identity();
            var stream = _eventStore.Get(request.Id);

            stream.Apply(identity);

            if (identity.Name.Equals(request.Name))
            {
                return;
            }

            var key = Identity.Key(identity.Name);
            var rekey = Identity.Key(request.Name);

            if (_keyStore.Contains(rekey) || !_keyStore.Contains(key))
            {
                return;
            }

            _keyStore.Rekey(key, rekey);

            stream.AddEvent(identity.SetName(request.Name));

            _eventStore.Save(stream);

            context.Message.WithResponse(new IdentityNameSet
            {
                Id = request.Id,
                Name = request.Name
            });
        }
    }
}