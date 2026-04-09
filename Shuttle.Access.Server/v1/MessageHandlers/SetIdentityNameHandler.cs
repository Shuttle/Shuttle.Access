using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetIdentityNameHandler(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IMessageHandler<SetIdentityName>
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