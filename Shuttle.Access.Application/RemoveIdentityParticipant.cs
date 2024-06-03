using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application
{
    public class RemoveIdentityParticipant : IAsyncParticipant<RemoveIdentity>
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

        public async Task ProcessMessageAsync(IParticipantContext<RemoveIdentity> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;
            var id = message.Id;
            var identity = new Identity();
            var stream = await _eventStore.GetAsync(id);

            stream.Apply(identity);

            stream.AddEvent(identity.Remove());

            await _keyStore.RemoveAsync(id);

            await _eventStore.SaveAsync(stream);
        }
    }
}