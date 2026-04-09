using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetRoleNameHandler(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IMessageHandler<SetRoleName>
{
    public async Task HandleAsync(SetRoleName message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Guard.AgainstNull(message).Name))
        {
            return;
        }

        var role = new Role();
        var stream = await eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(role);

        if (role.Name.Equals(message.Name))
        {
            return;
        }

        var key = Role.Key(role.Name, role.TenantId);
        var rekey = Role.Key(message.Name, role.TenantId);

        if (await idKeyRepository.ContainsAsync(rekey, cancellationToken) || !await idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await idKeyRepository.RekeyAsync(key, rekey, cancellationToken);

        stream.Add(role.SetName(message.Name));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}