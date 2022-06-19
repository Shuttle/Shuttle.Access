using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class SetIdentityRoleParticipant : IParticipant<RequestResponseMessage<SetIdentityRole, IdentityRoleSet>>
    {
        private readonly IEventStore _eventStore;

        public SetIdentityRoleParticipant(IEventStore eventStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));

            _eventStore = eventStore;
        }

        public void ProcessMessage(IParticipantContext<RequestResponseMessage<SetIdentityRole, IdentityRoleSet>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            var identity = new Identity();
            var request = message.Request;
            var stream = _eventStore.Get(request.IdentityId);

            stream.Apply(identity);

            if (request.Active && !identity.IsInRole(request.RoleId))
            {
                stream.AddEvent(identity.AddRole(request.RoleId));
            }

            if (!request.Active && identity.IsInRole(request.RoleId))
            {
                stream.AddEvent(identity.RemoveRole(request.RoleId));
            }

            message.WithResponse(new IdentityRoleSet
            {
                RoleId = request.RoleId,
                IdentityId = request.IdentityId,
                Active = request.Active,
                SequenceNumber = _eventStore.Save(stream)
            });
        }
    }
}