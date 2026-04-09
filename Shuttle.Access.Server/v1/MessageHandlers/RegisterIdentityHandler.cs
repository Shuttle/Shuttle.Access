using Shuttle.Access.Messages.v1;
using Shuttle.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RegisterIdentityHandler(IBus bus, IMediator mediator, ITenantQuery tenantQuery) : IMessageHandler<RegisterIdentity>
{
    public async Task HandleAsync(RegisterIdentity message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(mediator);
        ArgumentNullException.ThrowIfNull(tenantQuery);

        var registerIdentity = new Application.RegisterIdentity(message.Id, message.Name, message.Description, message.GeneratedPassword, message.PasswordHash, message.RegisteredBy, message.Activated, message.AuditTenantId, message.AuditIdentityName)
            .AddRoleIds(message.RoleIds)
            .AddTenantIds(message.TenantIds);

        await mediator.SendAsync(registerIdentity, cancellationToken);
    }
}