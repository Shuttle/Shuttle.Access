using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RemoveIdentityParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RemoveIdentity>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);

    public async Task HandleAsync(RemoveIdentity message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var id = message.Id;
        var identity = new Identity();
        var stream = await _eventStore.GetAsync(id, cancellationToken: cancellationToken);

        stream.Apply(identity);

        stream.Add(identity.Remove());

        await _idKeyRepository.RemoveAsync(id, cancellationToken);

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}