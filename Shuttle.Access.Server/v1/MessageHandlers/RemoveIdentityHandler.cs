using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RemoveIdentityHandler(IMediator mediator) : IMessageHandler<RemoveIdentity>
{
    public async Task HandleAsync(RemoveIdentity message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.RemoveIdentity(message.Id, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
