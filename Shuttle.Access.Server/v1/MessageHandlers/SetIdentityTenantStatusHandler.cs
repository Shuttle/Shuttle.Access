using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetIdentityTenantStatusHandler(IMediator mediator) : IMessageHandler<SetIdentityTenantStatus>
{
    public async Task HandleAsync(SetIdentityTenantStatus message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.SetIdentityTenantStatus(message.IdentityId, message.TenantId, message.Active, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
