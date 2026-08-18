using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetIdentityTenantStatusParticipant(IEventStore eventStore) : IParticipant<SetIdentityTenantStatus>
{
    public async Task HandleAsync(SetIdentityTenantStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var identity = new Identity();
        var stream = await eventStore.GetAsync(message.IdentityId, cancellationToken);

        stream.Apply(identity);

        if (message.Active && !identity.IsInTenant(message.TenantId))
        {
            stream.Add(identity.AddTenant(message.TenantId));
        }

        if (!message.Active && identity.IsInTenant(message.TenantId))
        {
            stream.Add(identity.RemoveTenant(message.TenantId));
        }

        if (stream.ShouldSave())
        {
            await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
        }
    }
}
