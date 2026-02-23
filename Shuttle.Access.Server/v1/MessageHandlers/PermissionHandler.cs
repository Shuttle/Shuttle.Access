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

    public async Task HandleAsync(RegisterPermission message, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsync(Guard.AgainstNull(message), cancellationToken);
    }

    public async Task HandleAsync(SetPermissionDescription message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Guard.AgainstNull(message).Description))
        {
            return;
        }

        await _mediator.SendAsync(message, cancellationToken);
    }

    public async Task HandleAsync(SetPermissionName message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        await _mediator.SendAsync(Guard.AgainstNull(message), cancellationToken);
    }

    public async Task HandleAsync(SetPermissionStatus message, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsync(Guard.AgainstNull(message), cancellationToken);
    }
}