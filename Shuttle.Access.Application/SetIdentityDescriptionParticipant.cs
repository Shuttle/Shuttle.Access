using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetIdentityDescriptionParticipant(IEventStore eventStore) : IParticipant<SetIdentityDescription>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task HandleAsync(SetIdentityDescription message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var identity = new Identity();
        var stream = await _eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(identity);

        if (identity.Description.Equals(message.Description))
        {
            return;
        }

        stream.Add(identity.SetDescription(message.Description));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}