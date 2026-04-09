using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetPermissionStatusHandler(IEventStore eventStore) : IMessageHandler<SetPermissionStatus>
{
    public async Task HandleAsync(SetPermissionStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);
        Guard.AgainstUndefinedEnum<PermissionStatus>(message.Status, nameof(message.Status));

        var stream = await eventStore.GetAsync(message.Id, cancellationToken);

        if (stream.IsEmpty)
        {
            return;
        }

        var permission = new Permission();

        stream.Apply(permission);

        var status = (PermissionStatus)message.Status;

        switch (status)
        {
            case PermissionStatus.Active:
            {
                stream.Add(permission.Activate());
                break;
            }
            case PermissionStatus.Deactivated:
            {
                stream.Add(permission.Deactivate());
                break;
            }
            case PermissionStatus.Removed:
            {
                stream.Add(permission.Remove());
                break;
            }
        }

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}