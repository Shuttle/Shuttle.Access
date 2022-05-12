using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;
using Shuttle.Recall;

namespace Shuttle.Access.Server.Handlers
{
    public class RoleHandler :
        IMessageHandler<RegisterRole>,
        IMessageHandler<RemoveRole>,
        IMessageHandler<SetRolePermissionStatus>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IEventStore _eventStore;
        private readonly IMediator _mediator;

        public RoleHandler(IDatabaseContextFactory databaseContextFactory, IEventStore eventStore, IMediator mediator)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(mediator, nameof(mediator));

            _databaseContextFactory = databaseContextFactory;
            _eventStore = eventStore;
            _mediator = mediator;
        }

        public void ProcessMessage(IHandlerContext<RegisterRole> context)
        {
            var message = context.Message;

            if (string.IsNullOrEmpty(message.Name))
            {
                return;
            }

            var requestResponse = new RequestResponseMessage<RegisterRole, RoleRegistered>(message);

            using (_databaseContextFactory.Create())
            {
                _mediator.Send(requestResponse);
            }

            context.Publish(requestResponse.Response);
        }

        public void ProcessMessage(IHandlerContext<RemoveRole> context)
        {
            var message = context.Message;

            Role role;

            using (_databaseContextFactory.Create())
            {
                role = new Role(message.Id);
                var stream = _eventStore.Get(message.Id);

                stream.Apply(role);

                stream.AddEvent(role.Remove());

                _eventStore.Save(stream);
            }

            context.Publish(new RoleRemoved
            {   
                Id = message.Id,
                Name = role.Name
            });
        }

        public void ProcessMessage(IHandlerContext<SetRolePermissionStatus> context)
        {
            var message = context.Message;

            using (_databaseContextFactory.Create())
            {
                var role = new Role(message.RoleId);
                var stream = _eventStore.Get(message.RoleId);

                stream.Apply(role);

                if (message.Active && !role.HasPermission(message.Permission))
                {
                    stream.AddEvent(role.AddPermission(message.Permission));
                }

                if (!message.Active && role.HasPermission(message.Permission))
                {
                    stream.AddEvent(role.RemovePermission(message.Permission));
                }

                _eventStore.Save(stream);
            }

            context.Publish(new RolePermissionStatusSet
            {
                RoleId = message.RoleId,
                Permission = message.Permission,
                Active = message.Active
            });
        }
    }
}