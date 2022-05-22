using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class SetIdentityNameParticipant : IParticipant<RequestResponseMessage<SetIdentityName, IdentityNameSet>>
    {
        private readonly IEventStore _eventStore;

        public SetIdentityNameParticipant(IEventStore eventStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));

            _eventStore = eventStore;
        }

        public void ProcessMessage(IParticipantContext<RequestResponseMessage<SetIdentityName, IdentityNameSet>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var request = context.Message.Request;
            var aggregate = new Identity();
            var stream = _eventStore.Get(request.Id);

            stream.Apply(aggregate);

            if (aggregate.Name.Equals(request.Name))
            {
                return;
            }

            stream.AddEvent(aggregate.SetName(request.Name));

            _eventStore.Save(stream);

            context.Message.WithResponse(new IdentityNameSet
            {
                Id = request.Id,
                Name = request.Name
            });
        }
    }
}