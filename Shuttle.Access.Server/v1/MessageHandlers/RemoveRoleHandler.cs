using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RemoveRoleHandler(IMediator mediator) : IMessageHandler<RemoveRole>
{
    public async Task HandleAsync(RemoveRole message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.RemoveRole(message.Id, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
