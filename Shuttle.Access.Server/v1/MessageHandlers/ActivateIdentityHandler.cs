using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class ActivateIdentityHandler(IMediator mediator) : IMessageHandler<ActivateIdentity>
{
    public async Task HandleAsync(ActivateIdentity message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.ActivateIdentity(message.Id, message.Name, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
