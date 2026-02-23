using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class SetIdentityNameParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<SetIdentityName>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);

    public async Task HandleAsync(SetIdentityName message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var identity = new Identity();
        var stream = await _eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(identity);

        if (identity.Name.Equals(message.Name))
        {
            return;
        }

        var key = Identity.Key(identity.Name);
        var rekey = Identity.Key(message.Name);

        if (await _idKeyRepository.ContainsAsync(rekey, cancellationToken) || !await _idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await _idKeyRepository.RekeyAsync(key, rekey, cancellationToken);

        stream.Add(identity.SetName(message.Name));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}