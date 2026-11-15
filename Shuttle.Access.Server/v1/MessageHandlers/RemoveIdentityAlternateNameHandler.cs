using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RemoveIdentityAlternateNameHandler(IMediator mediator) : IMessageHandler<RemoveIdentityAlternateName>
{
    public async Task HandleAsync(RemoveIdentityAlternateName message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.RemoveIdentityAlternateName(message.Id, message.Name, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
