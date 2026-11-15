using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class PromoteIdentityAlternateNameHandler(IMediator mediator) : IMessageHandler<PromoteIdentityAlternateName>
{
    public async Task HandleAsync(PromoteIdentityAlternateName message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.PromoteIdentityAlternateName(message.Id, message.Name, (IdentityAlternateNamePromotionMode)message.Mode, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
