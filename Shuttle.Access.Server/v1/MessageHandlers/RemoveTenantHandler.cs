using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RemoveTenantHandler(IMediator mediator) : IMessageHandler<RemoveTenant>
{
    public async Task HandleAsync(RemoveTenant message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.RemoveTenant(message.Id, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
