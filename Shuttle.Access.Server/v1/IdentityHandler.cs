using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.Server.v1
{
    public class IdentityHandler :
        IAsyncMessageHandler<RegisterIdentity>,
        IAsyncMessageHandler<SetIdentityRole>,
        IAsyncMessageHandler<RemoveIdentity>,
        IAsyncMessageHandler<SetPassword>,
        IAsyncMessageHandler<ActivateIdentity>,
        IAsyncMessageHandler<SetIdentityName>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IMediator _mediator;

        public IdentityHandler(IDatabaseContextFactory databaseContextFactory, IMediator mediator)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(mediator, nameof(mediator));

            _databaseContextFactory = databaseContextFactory;
            _mediator = mediator;
        }

        public async Task ProcessMessageAsync(IHandlerContext<ActivateIdentity> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var requestResponse = new RequestResponseMessage<ActivateIdentity, IdentityActivated>(context.Message);

            using (_databaseContextFactory.Create())
            {
                await _mediator.SendAsync(requestResponse);
            }

            if (requestResponse.Response != null)
            {
                context.Publish(requestResponse.Response);
            }
        }

        public async Task ProcessMessageAsync(IHandlerContext<RegisterIdentity> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            if (string.IsNullOrEmpty(message.Name) ||
                string.IsNullOrEmpty(message.RegisteredBy) ||
                message.PasswordHash == null ||
                message.PasswordHash.Length == 0)
            {
                return;
            }

            var requestResponse = new RequestResponseMessage<RegisterIdentity, IdentityRegistered>(message);

            using (_databaseContextFactory.Create())
            {
                await _mediator.SendAsync(requestResponse);
            }

            if (requestResponse.Response != null)
            {
                context.Publish(requestResponse.Response);
            }
        }

        public async Task ProcessMessageAsync(IHandlerContext<RemoveIdentity> context)
        {
            using (_databaseContextFactory.Create())
            {
                await _mediator.SendAsync(context.Message);

                context.Publish(new IdentityRemoved
                {
                    Id = context.Message.Id
                });
            }
        }

        public async Task ProcessMessageAsync(IHandlerContext<SetIdentityRole> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;
            var reviewRequest = new RequestMessage<SetIdentityRole>(message);
            var requestResponse = new RequestResponseMessage<SetIdentityRole, IdentityRoleSet>(message);

            using (_databaseContextFactory.Create())
            {
                await _mediator.SendAsync(reviewRequest);

                if (!reviewRequest.Ok)
                {
                    return;
                }

                await _mediator.SendAsync(requestResponse);

                if (requestResponse.Response != null)
                {
                    context.Publish(requestResponse.Response);
                }
            }
        }

        public async Task ProcessMessageAsync(IHandlerContext<SetPassword> context)
        {
            Guard.AgainstNull(context, nameof(context));

            using (_databaseContextFactory.Create())
            {
                await _mediator.SendAsync(context.Message);
            }
        }

        public async Task ProcessMessageAsync(IHandlerContext<SetIdentityName> context)
        {
            var message = context.Message;

            if (string.IsNullOrEmpty(message.Name))
            {
                return;
            }

            var requestResponse = new RequestResponseMessage<SetIdentityName, IdentityNameSet>(message);

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