using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetIdentityDescriptionParticipant(IEventStore eventStore) : IParticipant<RequestResponseMessage<SetIdentityDescription, IdentityDescriptionSet>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task ProcessMessageAsync(RequestResponseMessage<SetIdentityDescription, IdentityDescriptionSet> message, CancellationToken cancellationToken = default)
    {
        var request = Guard.AgainstNull(message).Request;

        var identity = new Identity();
        var stream = await _eventStore.GetAsync(request.Id, cancellationToken);

        stream.Apply(identity);

        if (identity.Description.Equals(request.Description))
        {
            return;
        }

        stream.Add(identity.SetDescription(request.Description));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(request), cancellationToken);

        message.WithResponse(new()
        {
            Id = request.Id,
            Description = request.Description
        });
    }
}