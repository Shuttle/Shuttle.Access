using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application;

public class SetPermissionDescriptionParticipant : IParticipant<RequestResponseMessage<SetPermissionDescription, PermissionDescriptionSet>>
{
    private readonly IEventStore _eventStore;
    private readonly IIdKeyRepository _idKeyRepository;

    public SetPermissionDescriptionParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository)
    {
        _eventStore = Guard.AgainstNull(eventStore);
        _idKeyRepository = Guard.AgainstNull(idKeyRepository);
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<SetPermissionDescription, PermissionDescriptionSet>> context)
    {
        Guard.AgainstNull(context);

        var request = context.Message.Request;

        var permission = new Permission();
        var stream = await _eventStore.GetAsync(request.Id);

        stream.Apply(permission);

        if (permission.Description.Equals(request.Description))
        {
            return;
        }

        stream.Add(permission.SetDescription(request.Description));

        await _eventStore.SaveAsync(stream);

        context.Message.WithResponse(new()
        {
            Id = request.Id,
            Description = request.Description
        });
    }
}