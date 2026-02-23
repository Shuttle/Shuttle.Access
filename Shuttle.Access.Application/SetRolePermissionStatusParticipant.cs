using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetRolePermissionStatusParticipant(IEventStore eventStore) : IParticipant<SetRolePermissionStatus>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task HandleAsync(SetRolePermissionStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var role = new Role();
        var stream = await _eventStore.GetAsync(message.RoleId, cancellationToken: cancellationToken);

        stream.Apply(role);

        if (message.Active && !role.HasPermission(message.PermissionId))
        {
            stream.Add(role.AddPermission(message.PermissionId));
        }

        if (!message.Active && role.HasPermission(message.PermissionId))
        {
            stream.Add(role.RemovePermission(message.PermissionId));
        }

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}