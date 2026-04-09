using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetRolePermissionStatusHandler(IEventStore eventStore) : IMessageHandler<SetRolePermissionStatus>
{
    public async Task HandleAsync(SetRolePermissionStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var role = new Role();
        var stream = await eventStore.GetAsync(message.RoleId, cancellationToken: cancellationToken);

        stream.Apply(role);

        if (message.Active && !role.HasPermission(message.PermissionId))
        {
            stream.Add(role.AddPermission(message.PermissionId));
        }

        if (!message.Active && role.HasPermission(message.PermissionId))
        {
            stream.Add(role.RemovePermission(message.PermissionId));
        }

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}