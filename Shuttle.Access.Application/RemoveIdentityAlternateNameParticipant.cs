using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RemoveIdentityAlternateNameParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RemoveIdentityAlternateName>
{
    public async Task HandleAsync(RemoveIdentityAlternateName message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var identity = new Identity();
        var stream = await eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(identity);

        stream.Add(identity.RemoveAlternateName(message.Name));

        await idKeyRepository.RemoveAsync(Identity.Key(message.Name), cancellationToken);

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
