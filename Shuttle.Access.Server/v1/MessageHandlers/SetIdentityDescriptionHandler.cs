using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetIdentityDescriptionHandler(IMediator mediator) : IMessageHandler<SetIdentityDescription>
{
    public async Task HandleAsync(SetIdentityDescription message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.SetIdentityDescription(message.Id, message.Description, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
