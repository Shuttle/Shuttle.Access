using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class AddIdentityAlternateNameHandler(IMediator mediator) : IMessageHandler<AddIdentityAlternateName>
{
    public async Task HandleAsync(AddIdentityAlternateName message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.AddIdentityAlternateName(message.Id, message.Name, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
