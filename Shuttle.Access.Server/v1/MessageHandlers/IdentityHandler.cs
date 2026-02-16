using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class IdentityHandler(IBus bus, IMediator mediator) :
    IMessageHandler<RegisterIdentity>,
    IMessageHandler<SetIdentityRoleStatus>,
    IMessageHandler<RemoveIdentity>,
    IMessageHandler<SetPassword>,
    IMessageHandler<ActivateIdentity>,
    IMessageHandler<SetIdentityName>,
    IMessageHandler<SetIdentityDescription>
{
    private readonly IBus _bus = Guard.AgainstNull(bus);
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);

    public async Task HandleAsync(ActivateIdentity message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var requestResponse = new RequestResponseMessage<ActivateIdentity, IdentityActivated>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await _bus.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task HandleAsync(RegisterIdentity message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

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
            await _bus.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task HandleAsync(RemoveIdentity message, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsync(message, cancellationToken);

        await _bus.PublishAsync(new IdentityRemoved
        {
            Id = message.Id
        }, cancellationToken: cancellationToken);
    }

    public async Task HandleAsync(SetIdentityDescription message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(message.Description))
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<SetIdentityDescription, IdentityDescriptionSet>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await _bus.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task HandleAsync(SetIdentityName message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<SetIdentityName, IdentityNameSet>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await _bus.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task HandleAsync(SetIdentityRoleStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

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
            await _bus.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task HandleAsync(SetPassword message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        await _mediator.SendAsync(message, cancellationToken);
    }
}