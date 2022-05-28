﻿using Shuttle.Access.Messages.v1;
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
            var key = Permission.Key(request.Name);
            var id = request.Id;

            if (_keyStore.Contains(key) || !_keyStore.Contains(id))
            {
                return;
            }

            _keyStore.Rekey(id, key);

            var permission = new Permission();
            var stream = _eventStore.Get(request.Id);

            stream.Apply(permission);

            if (permission.Name.Equals(request.Name))
            {
                return;
            }

            stream.AddEvent(permission.SetName(request.Name));

            _eventStore.Save(stream);

            context.Message.WithResponse(new PermissionNameSet
            {
                Id = request.Id,
                Name = request.Name
            });
        }
    }
}