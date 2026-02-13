using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class PermissionHandler(IBus bus, IMediator mediator) :
    IMessageHandler<RegisterPermission>,
    IMessageHandler<SetPermissionStatus>,
    IMessageHandler<SetPermissionName>,
    IMessageHandler<SetPermissionDescription>
{
    private readonly IBus _bus = Guard.AgainstNull(bus);
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);

    public async Task ProcessMessageAsync(RegisterPermission message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var requestResponse = new RequestResponseMessage<RegisterPermission, PermissionRegistered>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await _bus.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(SetPermissionDescription message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(message.Description))
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<SetPermissionDescription, PermissionDescriptionSet>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await _bus.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(SetPermissionName message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<SetPermissionName, PermissionNameSet>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await _bus.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(SetPermissionStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var requestResponse = new RequestResponseMessage<SetPermissionStatus, PermissionStatusSet>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await _bus.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }
}