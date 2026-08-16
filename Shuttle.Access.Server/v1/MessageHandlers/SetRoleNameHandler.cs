using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetRoleNameHandler(IMediator mediator) : IMessageHandler<SetRoleName>
{
    public async Task HandleAsync(SetRoleName message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.SetRoleName(message.Id, message.Name, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
