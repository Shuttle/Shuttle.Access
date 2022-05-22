using System;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application
{
    public class RegisterPermissionParticipant : IParticipant<RequestResponseMessage<RegisterPermission, PermissionRegistered>>
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

        public void ProcessMessage(IParticipantContext<RequestResponseMessage<RegisterPermission, PermissionRegistered>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message.Request;

            var key = Permission.Key(message.Name);

            if (_keyStore.Contains(key))
            {
                return;
            }

            var id = Guid.NewGuid();

            _keyStore.Add(id, key);

            var aggregate = new Permission();
            var stream = _eventStore.CreateEventStream(id);
            var status = message.Status;

            if (!Enum.IsDefined(typeof(PermissionStatus), status))
            {
                status = (int)PermissionStatus.Active;
            }

            stream.AddEvent(aggregate.Add(message.Name, (PermissionStatus)status));

            _eventStore.Save(stream);

            context.Message.WithResponse(new PermissionRegistered
            {
                Id = id,
                Name = message.Name
            });
        }
    }
}