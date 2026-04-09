using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RemoveRoleHandler(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IMessageHandler<RemoveRole>
{
    public async Task HandleAsync(RemoveRole message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var stream = await eventStore.GetAsync(message.Id, cancellationToken: cancellationToken);

        if (stream.IsEmpty)
        {
            return;
        }

        var role = new Role();

        stream.Apply(role);

        stream.Add(role.Remove());

        await idKeyRepository.RemoveAsync(message.Id, cancellationToken);

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}