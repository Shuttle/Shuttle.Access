using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetIdentityTenantStatusParticipant(IEventStore eventStore) : IParticipant<SetIdentityTenantStatus>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task HandleAsync(SetIdentityTenantStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var identity = new Identity();
        var stream = await _eventStore.GetAsync(message.IdentityId, cancellationToken);

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
            await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
        }
    }
}