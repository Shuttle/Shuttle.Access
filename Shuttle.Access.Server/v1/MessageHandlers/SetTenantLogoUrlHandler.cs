using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetTenantLogoUrlHandler(IMediator mediator) : IMessageHandler<SetTenantLogoUrl>
{
    public async Task HandleAsync(SetTenantLogoUrl message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.SetTenantLogoUrl(message.Id, message.LogoUrl, message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}
