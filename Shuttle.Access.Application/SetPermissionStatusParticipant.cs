using Azure.Core;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetPermissionStatusParticipant(IEventStore eventStore) : IParticipant<SetPermissionStatus>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task HandleAsync(SetPermissionStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);
        Guard.AgainstUndefinedEnum<PermissionStatus>(message.Status, nameof(message.Status));

        var stream = await _eventStore.GetAsync(message.Id, cancellationToken);

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

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}