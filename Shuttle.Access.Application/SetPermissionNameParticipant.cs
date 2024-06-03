using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application
{
    public class SetPermissionNameParticipant : IAsyncParticipant<RequestResponseMessage<SetPermissionName, PermissionNameSet>>
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

        public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<SetPermissionName, PermissionNameSet>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var request = context.Message.Request;

            var permission = new Permission();
            var stream = await _eventStore.GetAsync(request.Id);

            stream.Apply(permission);

            if (permission.Name.Equals(request.Name))
            {
                return;
            }

            var key = Permission.Key(permission.Name);
            var rekey = Permission.Key(request.Name);

            if (await _keyStore.ContainsAsync(rekey) || !await _keyStore.ContainsAsync(key))
            {
                return;
            }

            await _keyStore.RekeyAsync(key, rekey);

            stream.AddEvent(permission.SetName(request.Name));

            context.Message.WithResponse(new PermissionNameSet
            {
                Id = request.Id,
                Name = request.Name,
                SequenceNumber = await _eventStore.SaveAsync(stream)
            });
        }
    }
}