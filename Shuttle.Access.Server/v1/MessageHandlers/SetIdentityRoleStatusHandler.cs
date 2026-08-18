using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetIdentityRoleStatusHandler(IMediator mediator) : IMessageHandler<SetIdentityRoleStatus>
{
    public async Task HandleAsync(SetIdentityRoleStatus message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.SetIdentityRoleStatus(message.IdentityId, message.RoleId, message.Active, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
