using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class AddIdentityAlternateNameParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<AddIdentityAlternateName>
{
    public async Task HandleAsync(AddIdentityAlternateName message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var key = Identity.Key(message.Name);
        var existingId = await idKeyRepository.FindAsync(key, cancellationToken);

        if (existingId.HasValue && !existingId.Value.Equals(message.Id))
        {
            throw new ApplicationException(string.Format(Shuttle.Access.Resources.IdentityAlternateNameInUseException, message.Name));
        }

        var identity = new Identity();
        var stream = await eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(identity);

        stream.Add(identity.AddAlternateName(message.Name));

        if (!existingId.HasValue)
        {
            await idKeyRepository.AddAsync(message.Id, key, cancellationToken);
        }

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
