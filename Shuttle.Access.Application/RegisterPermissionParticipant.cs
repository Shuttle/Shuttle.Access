using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterPermissionParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RegisterPermission>
{
    public async Task HandleAsync(RegisterPermission message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(eventStore);
        ArgumentNullException.ThrowIfNull(idKeyRepository);

        var key = Permission.Key(message.Name);

        if (await idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await idKeyRepository.AddAsync(message.Id, key, cancellationToken);

        var stream = (await eventStore.GetAsync(message.Id, cancellationToken)).MustBeEmpty();
        var aggregate = stream.Get<Permission>();

        stream.Add(aggregate.Register(message.Name, message.Description, message.Status));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}