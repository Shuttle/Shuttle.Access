using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RegisterRoleHandler(IMediator mediator) : IMessageHandler<RegisterRole>
{
    public async Task HandleAsync(RegisterRole message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var registerRole = new Application.RegisterRole(message.Id, message.TenantId, message.Name, message.AuditTenantId, message.AuditIdentityName);

        foreach (var permission in message.Permissions)
        {
            registerRole.AddPermissionName(permission.Name);
        }

        await mediator.SendAsync(registerRole, cancellationToken);
    }
}
