using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetIdentityRoleStatusParticipant(IEventStore eventStore) : IParticipant<RequestResponseMessage<SetIdentityRoleStatus, IdentityRoleSet>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task ProcessMessageAsync(RequestResponseMessage<SetIdentityRoleStatus, IdentityRoleSet> message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var identity = new Identity();
        var request = message.Request;
        var stream = await _eventStore.GetAsync(request.IdentityId, cancellationToken);

        stream.Apply(identity);

        if (request.Active && !identity.IsInRole(request.RoleId))
        {
            stream.Add(identity.AddRole(request.RoleId));
        }

        if (!request.Active && identity.IsInRole(request.RoleId))
        {
            stream.Add(identity.RemoveRole(request.RoleId));
        }

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message.Request), cancellationToken);

        if (stream.ShouldSave())
        {
            message.WithResponse(new()
            {
                RoleId = request.RoleId,
                IdentityId = request.IdentityId,
                Active = request.Active,
                Version = stream.Version
            });
        }
    }
}