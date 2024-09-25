using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetPasswordParticipant : IAsyncParticipant<SetPassword>
{
    private readonly IEventStore _eventStore;

    public SetPasswordParticipant(IEventStore eventStore)
    {
        Guard.AgainstNull(eventStore, nameof(eventStore));

        _eventStore = eventStore;
    }

    public async Task ProcessMessageAsync(IParticipantContext<SetPassword> context)
    {
        Guard.AgainstNull(context, nameof(context));

        var message = context.Message;
        var identity = new Identity();
        var stream = await _eventStore.GetAsync(message.Id);

        stream.Apply(identity);
        stream.AddEvent(identity.SetPassword(message.PasswordHash));

        await _eventStore.SaveAsync(stream);
    }
}