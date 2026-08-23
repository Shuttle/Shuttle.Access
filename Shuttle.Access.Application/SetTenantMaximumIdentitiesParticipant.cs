using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetTenantMaximumIdentitiesParticipant(IEventStore eventStore) : IParticipant<SetTenantMaximumIdentities>
{
    public async Task HandleAsync(SetTenantMaximumIdentities message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var stream = (await eventStore.GetAsync(message.Id, cancellationToken)).MustHaveEvents();
        var tenant = stream.Get<Tenant>();

        if (tenant.MaximumIdentities == message.MaximumIdentities)
        {
            return;
        }

        stream.Add(tenant.SetMaximumIdentities(message.MaximumIdentities));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
