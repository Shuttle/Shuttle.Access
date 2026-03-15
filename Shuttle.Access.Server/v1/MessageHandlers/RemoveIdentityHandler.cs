using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RemoveIdentityHandler(IBus bus, IEventStore eventStore, IIdKeyRepository idKeyRepository) : IMessageHandler<RemoveIdentity>
{
    public async Task HandleAsync(RemoveIdentity message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(eventStore);
        ArgumentNullException.ThrowIfNull(idKeyRepository);

        var id = message.Id;
        var identity = new Identity();
        var stream = await eventStore.GetAsync(id, cancellationToken: cancellationToken);

        stream.Apply(identity);

        stream.Add(identity.Remove());

        await idKeyRepository.RemoveAsync(id, cancellationToken);

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);

        await bus.PublishAsync(new IdentityRemoved
        {
            Id = id,
            Name = identity.Name
        }, cancellationToken);
    }
}