using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class SetPermissionNameParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<SetPermissionName>
{
    public async Task HandleAsync(SetPermissionName message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        var permission = new Permission();
        var stream = await eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(permission);

        if (permission.Name.Equals(message.Name))
        {
            return;
        }

        var key = Permission.Key(permission.Name);
        var rekey = Permission.Key(message.Name);

        if (await idKeyRepository.ContainsAsync(rekey, cancellationToken) || !await idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await idKeyRepository.RekeyAsync(key, rekey, cancellationToken);

        stream.Add(permission.SetName(message.Name));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
