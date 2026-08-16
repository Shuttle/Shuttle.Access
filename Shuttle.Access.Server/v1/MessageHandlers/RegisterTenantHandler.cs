using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RegisterTenantHandler(IMediator mediator) : IMessageHandler<RegisterTenant>
{
    public async Task HandleAsync(RegisterTenant message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var registerTenant = new Application.RegisterTenant(message.Id, message.Name, (TenantStatus)message.Status, message.AuditTenantId, message.AuditIdentityName)
        {
            LogoUrl = message.LogoUrl,
            LogoSvg = message.LogoSvg,
            AdministratorIdentityName = message.AdministratorIdentityName,
            AccessAdministratorRoleId = message.AccessAdministratorRoleId
        };

        await mediator.SendAsync(registerTenant, cancellationToken);
    }
}
