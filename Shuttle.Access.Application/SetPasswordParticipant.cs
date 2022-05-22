using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class SetPasswordParticipant : IParticipant<SetPassword>
    {
        private readonly IEventStore _eventStore;

        public SetPasswordParticipant(IEventStore eventStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));

            _eventStore = eventStore;
        }

        public void ProcessMessage(IParticipantContext<SetPassword> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;
            var identity = new Identity();
            var stream = _eventStore.Get(message.Id);

            stream.Apply(identity);
            stream.AddEvent(identity.SetPassword(message.PasswordHash));

            _eventStore.Save(stream);
        }
    }
}