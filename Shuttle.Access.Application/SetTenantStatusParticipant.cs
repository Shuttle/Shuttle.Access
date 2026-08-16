using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetTenantStatusParticipant(IEventStore eventStore) : IParticipant<SetTenantStatus>
{
    public async Task HandleAsync(SetTenantStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var stream = (await eventStore.GetAsync(message.Id, cancellationToken)).MustHaveEvents();
        var aggregate = stream.Get<Tenant>();

        if (aggregate.Status == message.Status)
        {
            return;
        }

        stream.Add(aggregate.SetStatus(message.Status));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
