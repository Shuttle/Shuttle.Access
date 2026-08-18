using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RemoveIdentityParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RemoveIdentity>
{
    public async Task HandleAsync(RemoveIdentity message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(eventStore);
        ArgumentNullException.ThrowIfNull(idKeyRepository);

        var id = message.Id;
        var identity = new Identity();
        var stream = await eventStore.GetAsync(id, cancellationToken: cancellationToken);

        stream.Apply(identity);

        stream.Add(identity.Remove());

        await idKeyRepository.RemoveAsync(id, cancellationToken);

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
