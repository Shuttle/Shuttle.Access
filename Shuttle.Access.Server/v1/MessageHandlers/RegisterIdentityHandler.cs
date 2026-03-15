using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RegisterIdentityHandler(IBus bus, IMediator mediator) : IMessageHandler<RegisterIdentity>
{
    public async Task HandleAsync(RegisterIdentity message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(mediator);

        await mediator.SendAsync(new Application.RegisterIdentity(message.Id, message.Name, message.Description, message.GeneratedPassword, message.PasswordHash, message.RegisteredBy, message.Activated, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}