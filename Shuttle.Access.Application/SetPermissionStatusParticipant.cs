using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetPermissionStatusParticipant(IEventStore eventStore) : IParticipant<RequestResponseMessage<SetPermissionStatus, PermissionStatusSet>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task ProcessMessageAsync(RequestResponseMessage<SetPermissionStatus, PermissionStatusSet> message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);
        Guard.AgainstUndefinedEnum<PermissionStatus>(message.Request.Status, nameof(message.Request.Status));

        var stream = await _eventStore.GetAsync(message.Request.Id, cancellationToken: cancellationToken);

        if (stream.IsEmpty)
        {
            return;
        }

        var permission = new Permission();

        stream.Apply(permission);

        var status = (PermissionStatus)message.Request.Status;

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

        await _eventStore.SaveAsync(stream, cancellationToken);

        message.WithResponse(new()
        {
            Id = message.Request.Id,
            Name = permission.Name,
            Status = message.Request.Status,
            Version = stream.Version
        });
    }
}