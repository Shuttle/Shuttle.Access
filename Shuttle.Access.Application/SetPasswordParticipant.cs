using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetPasswordParticipant(IEventStore eventStore) : IParticipant<SetPassword>
{
    public async Task HandleAsync(SetPassword message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var identity = new Identity();
        var stream = await eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(identity);
        stream.Add(identity.SetPassword(message.PasswordHash));

        await eventStore.SaveAsync(stream, cancellationToken);
    }
}
