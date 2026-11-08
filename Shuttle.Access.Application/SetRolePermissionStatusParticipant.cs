using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetRolePermissionStatusParticipant(IEventStore eventStore, IPermissionQuery permissionQuery) : IParticipant<SetRolePermissionStatus>
{
    public async Task HandleAsync(SetRolePermissionStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var role = new Role();
        var stream = await eventStore.GetAsync(message.RoleId, cancellationToken: cancellationToken);

        stream.Apply(role);

        if (message.Active && !role.HasPermission(message.PermissionId))
        {
            // an unknown permission would be stored as an event that the role projection defers indefinitely (it waits
            // for the permission to be projected), which would stall the projection of every subsequent role event
            if (!await Guard.AgainstNull(permissionQuery).ContainsAsync(new Query.Permission.Specification().AddId(message.PermissionId), cancellationToken))
            {
                throw new ApplicationException($"Could not find a permission with id '{message.PermissionId}' for role '{message.RoleId}'.");
            }

            stream.Add(role.AddPermission(message.PermissionId));
        }

        if (!message.Active && role.HasPermission(message.PermissionId))
        {
            stream.Add(role.RemovePermission(message.PermissionId));
        }

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
