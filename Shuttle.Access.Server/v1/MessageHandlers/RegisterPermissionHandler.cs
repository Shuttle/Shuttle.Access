using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RegisterPermissionHandler(IMediator mediator) : IMessageHandler<RegisterPermission>
{
    public async Task HandleAsync(RegisterPermission message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(mediator);

        await mediator.SendAsync(new Application.RegisterPermission(message.Id, message.Name, message.Description, (PermissionStatus)message.Status, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}