using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetPermissionDescriptionParticipant(IEventStore eventStore) : IParticipant<RequestResponseMessage<SetPermissionDescription, PermissionDescriptionSet>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task ProcessMessageAsync(RequestResponseMessage<SetPermissionDescription, PermissionDescriptionSet> context, CancellationToken cancellationToken = default)
    {
        var request = Guard.AgainstNull(context).Request;

        var permission = new Permission();
        var stream = await _eventStore.GetAsync(request.Id, cancellationToken);

        stream.Apply(permission);

        if (permission.Description.Equals(request.Description))
        {
            return;
        }

        stream.Add(permission.SetDescription(request.Description));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(request), cancellationToken);

        context.WithResponse(new()
        {
            Id = request.Id,
            Description = request.Description
        });
    }
}