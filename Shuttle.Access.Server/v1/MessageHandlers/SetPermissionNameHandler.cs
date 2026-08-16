using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetPermissionNameHandler(IMediator mediator) : IMessageHandler<SetPermissionName>
{
    public async Task HandleAsync(SetPermissionName message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.SetPermissionName(message.Id, message.Name, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
