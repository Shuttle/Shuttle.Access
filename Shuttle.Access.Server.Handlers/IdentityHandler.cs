using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.Server.Handlers
{
    public class IdentityHandler :
        IMessageHandler<RegisterIdentity>,
        IMessageHandler<SetIdentityRoleStatus>,
        IMessageHandler<RemoveIdentity>,
        IMessageHandler<SetPassword>,
        IMessageHandler<ActivateIdentity>,
        IMessageHandler<SetIdentityName>
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

        public void ProcessMessage(IHandlerContext<ActivateIdentity> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var requestResponse = new RequestResponseMessage<ActivateIdentity, IdentityActivated>(context.Message);

            using (_databaseContextFactory.Create())
            {
                _mediator.Send(requestResponse);
            }

            context.Publish(requestResponse.Response);
        }

        public void ProcessMessage(IHandlerContext<RegisterIdentity> context)
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
                _mediator.Send(requestResponse);
            }

            if (requestResponse.Response != null)
            {
                context.Publish(requestResponse.Response);
            }
        }

        public void ProcessMessage(IHandlerContext<RemoveIdentity> context)
        {
            using (_databaseContextFactory.Create())
            {
                _mediator.Send(context.Message);

                context.Publish(new IdentityRemoved
                {
                    Id = context.Message.Id
                });
            }
        }

        public void ProcessMessage(IHandlerContext<SetIdentityRoleStatus> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            using (_databaseContextFactory.Create())
            {
                var reviewRequest = new RequestMessage<SetIdentityRoleStatus>(message);

                _mediator.Send(reviewRequest);

                if (!reviewRequest.Ok)
                {
                    return;
                }

                _mediator.Send(message);
            }
        }

        public void ProcessMessage(IHandlerContext<SetPassword> context)
        {
            Guard.AgainstNull(context, nameof(context));

            using (_databaseContextFactory.Create())
            {
                _mediator.Send(context.Message);
            }
        }

        public void ProcessMessage(IHandlerContext<SetIdentityName> context)
        {
            var message = context.Message;

            if (string.IsNullOrEmpty(message.Name))
            {
                return;
            }

            var requestResponse = new RequestResponseMessage<SetIdentityName, IdentityNameSet>(message);

            using (_databaseContextFactory.Create())
            {
                _mediator.Send(requestResponse);
            }

            if (requestResponse.Response == null)
            {
                return;
            }

            context.Publish(requestResponse.Response);
        }
    }
}