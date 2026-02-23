using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RemoveTenantParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RemoveTenant>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);

    public async Task HandleAsync(RemoveTenant message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var stream = await _eventStore.GetAsync(message.Id, cancellationToken: cancellationToken);

        if (stream.IsEmpty)
        {
            return;
        }

        var aggregate = new Tenant();

        stream.Apply(aggregate);

        stream.Add(aggregate.Remove());

        await _idKeyRepository.RemoveAsync(message.Id, cancellationToken);

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}