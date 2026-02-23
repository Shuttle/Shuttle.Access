using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetPermissionDescriptionParticipant(IEventStore eventStore) : IParticipant<SetPermissionDescription>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task HandleAsync(SetPermissionDescription message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var permission = new Permission();
        var stream = await _eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(permission);

        if (permission.Description.Equals(message.Description))
        {
            return;
        }

        stream.Add(permission.SetDescription(message.Description));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}