using System;
using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application
{
    public class RegisterPermissionParticipant : IAsyncParticipant<RequestResponseMessage<RegisterPermission, PermissionRegistered>>
    {
        private readonly IEventStore _eventStore;
        private readonly IKeyStore _keyStore;

        public RegisterPermissionParticipant(IEventStore eventStore, IKeyStore keyStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(keyStore, nameof(keyStore));

            _eventStore = eventStore;
            _keyStore = keyStore;
        }

        public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<RegisterPermission, PermissionRegistered>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message.Request;

            var key = Permission.Key(message.Name);

            if (await _keyStore.ContainsAsync(key))
            {
                return;
            }

            var id = Guid.NewGuid();

            await _keyStore.AddAsync(id, key);

            var aggregate = new Permission();
            var stream = await _eventStore.GetAsync(id);
            var status = message.Status;

            if (!Enum.IsDefined(typeof(PermissionStatus), status))
            {
                status = (int)PermissionStatus.Active;
            }

            stream.AddEvent(aggregate.Register(message.Name, (PermissionStatus)status));

            context.Message.WithResponse(new PermissionRegistered
            {
                Id = id,
                Name = message.Name,
                SequenceNumber = await _eventStore.SaveAsync(stream)
            });
        }
    }
}