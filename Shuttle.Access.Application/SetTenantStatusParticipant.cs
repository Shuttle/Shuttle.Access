using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetTenantStatusParticipant(IEventStore eventStore) : IParticipant<SetTenantStatus>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task HandleAsync(SetTenantStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var stream = await _eventStore.GetAsync(message.Id, cancellationToken);

        if (stream.IsEmpty)
        {
            return;
        }

        var tenant = new Tenant();

        stream.Apply(tenant);

        if (tenant.Status == message.Status)
        {
            return;
        }

        stream.Add(tenant.SetStatus(message.Status));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
