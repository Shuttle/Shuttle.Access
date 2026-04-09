using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetIdentityDescriptionHandler(IEventStore eventStore) : IMessageHandler<SetIdentityDescription>
{
    public async Task HandleAsync(SetIdentityDescription message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Guard.AgainstNull(message).Description))
        {
            return;
        }

        var identity = new Identity();
        var stream = await eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(identity);

        if (identity.Description.Equals(message.Description))
        {
            return;
        }

        stream.Add(identity.SetDescription(message.Description));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}