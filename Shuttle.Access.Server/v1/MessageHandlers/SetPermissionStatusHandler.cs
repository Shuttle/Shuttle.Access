using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetPermissionStatusHandler(IMediator mediator) : IMessageHandler<SetPermissionStatus>
{
    public async Task HandleAsync(SetPermissionStatus message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.SetPermissionStatus(message.Id, (PermissionStatus)message.Status, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
