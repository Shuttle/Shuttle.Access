using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetIdentityDescriptionParticipant(IEventStore eventStore) : IParticipant<SetIdentityDescription>
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
