using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1
{
    public class RoleHandler :
        IAsyncMessageHandler<RegisterRole>,
        IAsyncMessageHandler<RemoveRole>,
        IAsyncMessageHandler<SetRolePermission>,
        IAsyncMessageHandler<SetRoleName>
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

        public async Task ProcessMessageAsync(IHandlerContext<RegisterRole> context)
        {
            var message = context.Message;

            if (string.IsNullOrEmpty(message.Name))
            {
                return;
            }

            var requestResponse = new RequestResponseMessage<RegisterRole, RoleRegistered>(message);

            using (_databaseContextFactory.Create())
            {
                await _mediator.SendAsync(requestResponse);
            }

            if (requestResponse.Response != null)
            {
                context.Publish(requestResponse.Response);
            }
        }

        public async Task ProcessMessageAsync(IHandlerContext<RemoveRole> context)
        {
            var message = context.Message;

            var requestResponse = new RequestResponseMessage<RemoveRole, RoleRemoved>(message);

            using (_databaseContextFactory.Create())
            {
                await _mediator.SendAsync(requestResponse);
            }

            if (requestResponse.Response != null)
            {
                context.Publish(requestResponse.Response);
            }
        }

        public async Task ProcessMessageAsync(IHandlerContext<SetRolePermission> context)
        {
            var message = context.Message;

            var requestResponse = new RequestResponseMessage<SetRolePermission, RolePermissionSet>(message);

            using (_databaseContextFactory.Create())
            {
                await _mediator.SendAsync(requestResponse);
            }

            if (requestResponse.Response != null)
            {
                context.Publish(requestResponse.Response);
            }
        }

        public async Task ProcessMessageAsync(IHandlerContext<SetRoleName> context)
        {
            var message = context.Message;

            if (string.IsNullOrEmpty(message.Name))
            {
                return;
            }

            var requestResponse = new RequestResponseMessage<SetRoleName, RoleNameSet>(message);

            using (_databaseContextFactory.Create())
            {
                await _mediator.SendAsync(requestResponse);
            }

            if (requestResponse.Response != null)
            {
                context.Publish(requestResponse.Response);
            }
        }
    }
}