using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class SetIdentityNameParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<SetIdentityName>
{
    public async Task HandleAsync(SetIdentityName message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Guard.AgainstNull(message).Name))
        {
            return;
        }

        var identity = new Identity();
        var stream = await eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(identity);

        if (identity.Name.Equals(message.Name))
        {
            return;
        }

        var key = Identity.Key(identity.Name);
        var rekey = Identity.Key(message.Name);

        if (await idKeyRepository.ContainsAsync(rekey, cancellationToken) || !await idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await idKeyRepository.RekeyAsync(key, rekey, cancellationToken);

        stream.Add(identity.SetName(message.Name));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
