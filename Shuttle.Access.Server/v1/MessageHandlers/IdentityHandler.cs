using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class IdentityHandler(IMediator mediator) :
    IMessageHandler<RegisterIdentity>,
    IMessageHandler<SetIdentityRoleStatus>,
    IMessageHandler<RemoveIdentity>,
    IMessageHandler<SetPassword>,
    IMessageHandler<ActivateIdentity>,
    IMessageHandler<SetIdentityName>,
    IMessageHandler<SetIdentityDescription>
{
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);

    public async Task ProcessMessageAsync(IHandlerContext<ActivateIdentity> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var requestResponse = new RequestResponseMessage<ActivateIdentity, IdentityActivated>(context.Message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<RegisterIdentity> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var message = context.Message;

        if (string.IsNullOrEmpty(message.Name) ||
            string.IsNullOrEmpty(message.AuditIdentityName) ||
            message.PasswordHash.Length == 0)
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<RegisterIdentity, IdentityRegistered>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<RemoveIdentity> context, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsync(context.Message, cancellationToken);

        await context.PublishAsync(new IdentityRemoved
        {
            Id = context.Message.Id
        }, cancellationToken: cancellationToken);
    }

    public async Task ProcessMessageAsync(IHandlerContext<SetIdentityDescription> context, CancellationToken cancellationToken = default)
    {
        var message = context.Message;

        if (string.IsNullOrEmpty(message.Description))
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<SetIdentityDescription, IdentityDescriptionSet>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<SetIdentityName> context, CancellationToken cancellationToken = default)
    {
        var message = context.Message;

        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<SetIdentityName, IdentityNameSet>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<SetIdentityRoleStatus> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var message = context.Message;
        var reviewRequest = new RequestMessage<SetIdentityRoleStatus>(message);
        var requestResponse = new RequestResponseMessage<SetIdentityRoleStatus, IdentityRoleSet>(message);

        await _mediator.SendAsync(reviewRequest, cancellationToken);

        if (!reviewRequest.Ok)
        {
            return;
        }

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<SetPassword> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _mediator.SendAsync(context.Message, cancellationToken);
    }
}