using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetIdentityRoleStatusParticipant(IEventStore eventStore) : IParticipant<SetIdentityRoleStatus>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task HandleAsync(SetIdentityRoleStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var identity = new Identity();
        var stream = await _eventStore.GetAsync(message.IdentityId, cancellationToken);

        stream.Apply(identity);

        if (message.Active && !identity.IsInRole(message.RoleId))
        {
            stream.Add(identity.AddRole(message.RoleId));
        }

        if (!message.Active && identity.IsInRole(message.RoleId))
        {
            stream.Add(identity.RemoveRole(message.RoleId));
        }

        if (stream.ShouldSave())
        {
            await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
        }
    }
}