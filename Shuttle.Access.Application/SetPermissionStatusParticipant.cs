using Azure.Core;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetPermissionStatusParticipant(IEventStore eventStore) : IParticipant<RequestResponseMessage<SetPermissionStatus, PermissionStatusSet>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task HandleAsync(RequestResponseMessage<SetPermissionStatus, PermissionStatusSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);
        Guard.AgainstUndefinedEnum<PermissionStatus>(context.Request.Status, nameof(context.Request.Status));

        var stream = await _eventStore.GetAsync(context.Request.Id, cancellationToken);

        if (stream.IsEmpty)
        {
            return;
        }

        var permission = new Permission();

        stream.Apply(permission);

        var status = (PermissionStatus)context.Request.Status;

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

        await _eventStore.SaveAsync(stream, builder => builder.Audit(context.Request), cancellationToken);

        context.WithResponse(new()
        {
            Id = context.Request.Id,
            Name = permission.Name,
            Status = context.Request.Status,
            Version = stream.Version
        });
    }
}