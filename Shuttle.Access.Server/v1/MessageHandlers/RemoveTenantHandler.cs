using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RemoveTenantHandler(IEventStore eventStore, IIdKeyRepository idKeyRepository) :
    IMessageHandler<RemoveTenant>
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