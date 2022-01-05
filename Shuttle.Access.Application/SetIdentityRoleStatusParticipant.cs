using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class SetIdentityRoleStatusParticipant : IParticipant<SetIdentityRoleStatus>
    {
        private readonly IEventStore _eventStore;

        public SetIdentityRoleStatusParticipant(IEventStore eventStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));

            _eventStore = eventStore;
        }

        public void ProcessMessage(IParticipantContext<SetIdentityRoleStatus> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            var identity = new Identity(message.IdentityId);
            var stream = _eventStore.Get(message.IdentityId);

            stream.Apply(identity);

            if (message.Active && !identity.IsInRole(message.RoleId))
            {
                stream.AddEvent(identity.AddRole(message.RoleId));
            }

            if (!message.Active && identity.IsInRole(message.RoleId))
            {
                stream.AddEvent(identity.RemoveRole(message.RoleId));
            }

            _eventStore.Save(stream);
        }
    }
}