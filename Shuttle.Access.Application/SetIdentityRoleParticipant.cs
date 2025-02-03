using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetIdentityRoleParticipant : IParticipant<RequestResponseMessage<SetIdentityRole, IdentityRoleSet>>
{
    private readonly IEventStore _eventStore;

    public SetIdentityRoleParticipant(IEventStore eventStore)
    {
        _eventStore = Guard.AgainstNull(eventStore);
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<SetIdentityRole, IdentityRoleSet>> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message;

        var identity = new Identity();
        var request = message.Request;
        var stream = await _eventStore.GetAsync(request.IdentityId);

        stream.Apply(identity);

        if (request.Active && !identity.IsInRole(request.RoleId))
        {
            stream.Add(identity.AddRole(request.RoleId));
        }

        if (!request.Active && identity.IsInRole(request.RoleId))
        {
            stream.Add(identity.RemoveRole(request.RoleId));
        }

        if (stream.ShouldSave())
        {
            message.WithResponse(new()
            {
                RoleId = request.RoleId,
                IdentityId = request.IdentityId,
                Active = request.Active,
                SequenceNumber = await _eventStore.SaveAsync(stream)
            });
        }
    }
}