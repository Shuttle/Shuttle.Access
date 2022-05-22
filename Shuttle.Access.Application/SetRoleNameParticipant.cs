using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class SetRoleNameParticipant : IParticipant<RequestResponseMessage<SetRoleName, RoleNameSet>>
    {
        private readonly IEventStore _eventStore;

        public SetRoleNameParticipant(IEventStore eventStore)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));

            _eventStore = eventStore;
        }

        public void ProcessMessage(IParticipantContext<RequestResponseMessage<SetRoleName, RoleNameSet>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var request = context.Message.Request;
            var role = new Role();
            var stream = _eventStore.Get(request.Id);

            stream.Apply(role);

            if (role.Name.Equals(request.Name))
            {
                return;
            }

            stream.AddEvent(role.SetName(request.Name));

            _eventStore.Save(stream);

            context.Message.WithResponse(new RoleNameSet
            {
                Id = request.Id,
                Name = request.Name
            });
        }
    }
}