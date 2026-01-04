using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class PermissionHandler(IMediator mediator) :
    IMessageHandler<RegisterPermission>,
    IMessageHandler<SetPermissionStatus>,
    IMessageHandler<SetPermissionName>,
    IMessageHandler<SetPermissionDescription>
{
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);

    public async Task ProcessMessageAsync(IHandlerContext<RegisterPermission> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var message = context.Message;

        var requestResponse = new RequestResponseMessage<RegisterPermission, PermissionRegistered>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<SetPermissionDescription> context, CancellationToken cancellationToken = default)
    {
        var message = context.Message;

        if (string.IsNullOrEmpty(message.Description))
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<SetPermissionDescription, PermissionDescriptionSet>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<SetPermissionName> context, CancellationToken cancellationToken = default)
    {
        var message = context.Message;

        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<SetPermissionName, PermissionNameSet>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<SetPermissionStatus> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var message = context.Message;

        var requestResponse = new RequestResponseMessage<SetPermissionStatus, PermissionStatusSet>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }
}