using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetPermissionDescriptionParticipant(IEventStore eventStore) : IParticipant<SetPermissionDescription>
{
    public async Task HandleAsync(SetPermissionDescription message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Guard.AgainstNull(message).Description))
        {
            return;
        }

        var permission = new Permission();
        var stream = await eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(permission);

        if (permission.Description.Equals(message.Description))
        {
            return;
        }

        stream.Add(permission.SetDescription(message.Description));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
