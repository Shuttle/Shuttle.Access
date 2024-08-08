using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.Server.v1
{
    public class PermissionHandler :
        IAsyncMessageHandler<RegisterPermission>,
        IAsyncMessageHandler<SetPermissionStatus>,
        IAsyncMessageHandler<SetPermissionName>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IPermissionQuery _permissionQuery;
        private readonly IMediator _mediator;

        public PermissionHandler(IDatabaseContextFactory databaseContextFactory, IPermissionQuery permissionQuery,
            IMediator mediator)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(permissionQuery, nameof(permissionQuery));
            Guard.AgainstNull(mediator, nameof(mediator));

            _databaseContextFactory = databaseContextFactory;
            _permissionQuery = permissionQuery;
            _mediator = mediator;
        }

        public async Task ProcessMessageAsync(IHandlerContext<RegisterPermission> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            var requestResponse = new RequestResponseMessage<RegisterPermission, PermissionRegistered>(message);

            using (_databaseContextFactory.Create())
            {
                await _mediator.SendAsync(requestResponse);
            }

            if (requestResponse.Response != null)
            {
                context.Publish(requestResponse.Response);
            }
        }

        public async Task ProcessMessageAsync(IHandlerContext<SetPermissionStatus> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            var requestResponse = new RequestResponseMessage<SetPermissionStatus, PermissionStatusSet>(message);

            using (_databaseContextFactory.Create())
            {
                await _mediator.SendAsync(requestResponse);
            }

            if (requestResponse.Response != null)
            {
                context.Publish(requestResponse.Response);
            }
        }

        public async Task ProcessMessageAsync(IHandlerContext<SetPermissionName> context)
        {
            var message = context.Message;

            if (string.IsNullOrEmpty(message.Name))
            {
                return;
            }

            var requestResponse = new RequestResponseMessage<SetPermissionName, PermissionNameSet>(message);

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