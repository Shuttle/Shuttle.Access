using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application
{
    public class RemoveIdentityParticipant : IParticipant<RemoveIdentity>
    {
        private readonly IEventStore _eventStore;
        private readonly IKeyStore _keyStore;

        public RemoveIdentityParticipant(IEventStore eventStore, IKeyStore keyStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(keyStore, nameof(keyStore));

            _eventStore = eventStore;
            _keyStore = keyStore;
        }

        public void ProcessMessage(IParticipantContext<RemoveIdentity> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;
            var id = message.Id;
            var identity = new Identity();
            var stream = _eventStore.Get(id);

            stream.Apply(identity);

            stream.AddEvent(identity.Remove());

            _keyStore.Remove(id);

            _eventStore.Save(stream);
        }
    }
}