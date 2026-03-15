using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetPasswordHandler(IEventStore eventStore) : IMessageHandler<SetPassword>
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