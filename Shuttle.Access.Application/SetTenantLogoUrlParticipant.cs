using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetTenantLogoUrlParticipant(IEventStore eventStore) : IParticipant<SetTenantLogoUrl>
{
    public async Task HandleAsync(SetTenantLogoUrl message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var stream = (await eventStore.GetAsync(message.Id, cancellationToken)).MustHaveEvents();
        var tenant = stream.Get<Tenant>();

        if (tenant.LogoUrl == message.LogoUrl)
        {
            return;
        }

        stream.Add(tenant.SetLogoUrl(message.LogoUrl));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
