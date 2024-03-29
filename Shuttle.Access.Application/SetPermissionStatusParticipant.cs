﻿using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class SetPermissionStatusParticipant : IParticipant<RequestResponseMessage<SetPermissionStatus, PermissionStatusSet>>
    {
        private readonly IEventStore _eventStore;

        public SetPermissionStatusParticipant(IEventStore eventStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));

            _eventStore = eventStore;
        }

        public void ProcessMessage(
            IParticipantContext<RequestResponseMessage<SetPermissionStatus, PermissionStatusSet>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;
            
            Guard.AgainstUndefinedEnum<PermissionStatus>(message.Request.Status, nameof(message.Request.Status));

            var stream = _eventStore.Get(message.Request.Id);

            if (stream.IsEmpty)
            {
                return;
            }

            var permission = new Permission();

            stream.Apply(permission);

            var status = (PermissionStatus)message.Request.Status;

            switch (status)
            {
                case PermissionStatus.Active:
                {
                    stream.AddEvent(permission.Activate());
                    break;
                }
                case PermissionStatus.Deactivated:
                {
                    stream.AddEvent(permission.Deactivate());
                    break;
                }
                case PermissionStatus.Removed:
                {
                    stream.AddEvent(permission.Remove());
                    break;
                }
            }

            context.Message.WithResponse(new PermissionStatusSet
            {
                Id = message.Request.Id,
                Name = permission.Name,
                Status = message.Request.Status,
                SequenceNumber = _eventStore.Save(stream)
            });
        }
    }
}