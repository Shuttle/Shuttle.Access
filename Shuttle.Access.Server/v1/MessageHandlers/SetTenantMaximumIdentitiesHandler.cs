using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetTenantMaximumIdentitiesHandler(IMediator mediator) : IMessageHandler<SetTenantMaximumIdentities>
{
    public async Task HandleAsync(SetTenantMaximumIdentities message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.SetTenantMaximumIdentities(message.Id, message.MaximumIdentities, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
