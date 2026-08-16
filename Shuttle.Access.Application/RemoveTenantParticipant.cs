using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RemoveTenantParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RemoveTenant>
{
    public async Task HandleAsync(RemoveTenant message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var stream = await eventStore.GetAsync(message.Id, cancellationToken: cancellationToken);

        if (stream.IsEmpty)
        {
            return;
        }

        var aggregate = new Tenant();

        stream.Apply(aggregate);

        stream.Add(aggregate.Remove());

        await idKeyRepository.RemoveAsync(message.Id, cancellationToken);

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
