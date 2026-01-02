using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetRolePermissionParticipant(IEventStore eventStore) : IParticipant<RequestResponseMessage<SetRolePermission, RolePermissionSet>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task ProcessMessageAsync(RequestResponseMessage<SetRolePermission, RolePermissionSet> message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var request = message.Request;

        var role = new Role();
        var stream = await _eventStore.GetAsync(request.RoleId, cancellationToken: cancellationToken);

        stream.Apply(role);

        if (request.Active && !role.HasPermission(request.PermissionId))
        {
            stream.Add(role.AddPermission(request.PermissionId));
        }

        if (!request.Active && role.HasPermission(request.PermissionId))
        {
            stream.Add(role.RemovePermission(request.PermissionId));
        }

        await _eventStore.SaveAsync(stream, cancellationToken);

        message.WithResponse(new()
        {
            RoleId = request.RoleId,
            PermissionId = request.PermissionId,
            Active = request.Active,
            Version = stream.Version
        });
    }
}