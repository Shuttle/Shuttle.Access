using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetPermissionNameHandler(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IMessageHandler<SetPermissionName>
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