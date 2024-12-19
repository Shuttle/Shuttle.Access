using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetRolePermissionParticipant : IParticipant<RequestResponseMessage<SetRolePermission, RolePermissionSet>>
{
    private readonly IEventStore _eventStore;

    public SetRolePermissionParticipant(IEventStore eventStore)
    {
        Guard.AgainstNull(eventStore);

        _eventStore = eventStore;
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<SetRolePermission, RolePermissionSet>> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message;
        var request = message.Request;

        var role = new Role();
        var stream = await _eventStore.GetAsync(request.RoleId);

        stream.Apply(role);

        if (request.Active && !role.HasPermission(request.PermissionId))
        {
            stream.Add(role.AddPermission(request.PermissionId));
        }

        if (!request.Active && role.HasPermission(request.PermissionId))
        {
            stream.Add(role.RemovePermission(request.PermissionId));
        }

        message.WithResponse(new()
        {
            RoleId = request.RoleId,
            PermissionId = request.PermissionId,
            Active = request.Active,
            SequenceNumber = await _eventStore.SaveAsync(stream)
        });
    }
}