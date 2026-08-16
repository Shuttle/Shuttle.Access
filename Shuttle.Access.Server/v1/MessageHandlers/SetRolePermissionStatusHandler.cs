using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetRolePermissionStatusHandler(IMediator mediator) : IMessageHandler<SetRolePermissionStatus>
{
    public async Task HandleAsync(SetRolePermissionStatus message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.SetRolePermissionStatus(message.RoleId, message.PermissionId, message.Active, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
