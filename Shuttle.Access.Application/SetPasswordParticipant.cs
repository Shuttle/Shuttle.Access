using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetPasswordParticipant(IEventStore eventStore) : IParticipant<SetPassword>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task HandleAsync(SetPassword message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var identity = new Identity();
        var stream = await _eventStore.GetAsync(message.Id, cancellationToken: cancellationToken);

        stream.Apply(identity);
        stream.Add(identity.SetPassword(message.PasswordHash));

        await _eventStore.SaveAsync(stream, cancellationToken);
    }
}