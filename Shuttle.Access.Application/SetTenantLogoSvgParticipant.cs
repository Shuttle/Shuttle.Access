using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetTenantLogoSvgParticipant(IEventStore eventStore) : IParticipant<SetTenantLogoSvg>
{
    public async Task HandleAsync(SetTenantLogoSvg message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var stream = (await eventStore.GetAsync(message.Id, cancellationToken)).MustHaveEvents();
        var tenant = stream.Get<Tenant>();

        if (tenant.LogoSvg == message.LogoSvg)
        {
            return;
        }

        stream.Add(tenant.SetLogoSvg(message.LogoSvg));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
