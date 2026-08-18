using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RemoveRoleParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RemoveRole>
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
