using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetPermissionDescriptionHandler(IMediator mediator) : IMessageHandler<SetPermissionDescription>
{
    public async Task HandleAsync(SetPermissionDescription message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.SetPermissionDescription(message.Id, message.Description, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
