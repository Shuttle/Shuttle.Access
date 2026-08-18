using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetPermissionStatusParticipant(IEventStore eventStore) : IParticipant<SetPermissionStatus>
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

        switch (message.Status)
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
