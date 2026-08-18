using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetIdentityNameHandler(IMediator mediator) : IMessageHandler<SetIdentityName>
{
    public async Task HandleAsync(SetIdentityName message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.SetIdentityName(message.Id, message.Name, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
