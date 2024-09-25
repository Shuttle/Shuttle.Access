using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application;

public class RemoveRoleParticipant : IAsyncParticipant<RequestResponseMessage<RemoveRole, RoleRemoved>>
{
    private readonly IEventStore _eventStore;
    private readonly IKeyStore _keyStore;

    public RemoveRoleParticipant(IEventStore eventStore, IKeyStore keyStore)
    {
        Guard.AgainstNull(eventStore, nameof(eventStore));
        Guard.AgainstNull(keyStore, nameof(keyStore));

        _eventStore = eventStore;
        _keyStore = keyStore;
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<RemoveRole, RoleRemoved>> context)
    {
        Guard.AgainstNull(context, nameof(context));

        var message = context.Message;

        var stream = await _eventStore.GetAsync(message.Request.Id);

        if (stream.IsEmpty)
        {
            return;
        }

        var role = new Role();

        stream.Apply(role);

        stream.AddEvent(role.Remove());

        await _keyStore.RemoveAsync(message.Request.Id);

        await _eventStore.SaveAsync(stream);

        context.Message.WithResponse(new()
        {
            Id = message.Request.Id,
            Name = role.Name,
            SequenceNumber = await _eventStore.SaveAsync(stream)
        });
    }
}