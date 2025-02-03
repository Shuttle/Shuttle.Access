using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetPermissionStatusParticipant : IParticipant<RequestResponseMessage<SetPermissionStatus, PermissionStatusSet>>
{
    private readonly IEventStore _eventStore;

    public SetPermissionStatusParticipant(IEventStore eventStore)
    {
        _eventStore = Guard.AgainstNull(eventStore);
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<SetPermissionStatus, PermissionStatusSet>> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message;

        Guard.AgainstUndefinedEnum<PermissionStatus>(message.Request.Status, nameof(message.Request.Status));

        var stream = await _eventStore.GetAsync(message.Request.Id);

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

        context.Message.WithResponse(new()
        {
            Id = message.Request.Id,
            Name = permission.Name,
            Status = message.Request.Status,
            SequenceNumber = await _eventStore.SaveAsync(stream)
        });
    }
}