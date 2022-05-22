using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class RemoveIdentityParticipant : IParticipant<RemoveIdentity>
    {
        private readonly IEventStore _eventStore;

        public RemoveIdentityParticipant(IEventStore eventStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));

            _eventStore = eventStore;
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

            _eventStore.Save(stream);
        }
    }
}