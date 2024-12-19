using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application;

public class SetPermissionNameParticipant : IParticipant<RequestResponseMessage<SetPermissionName, PermissionNameSet>>
{
    private readonly IEventStore _eventStore;
    private readonly IIdKeyRepository _idKeyRepository;

    public SetPermissionNameParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository)
    {
        Guard.AgainstNull(eventStore);
        Guard.AgainstNull(idKeyRepository);

        _eventStore = eventStore;
        _idKeyRepository = idKeyRepository;
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<SetPermissionName, PermissionNameSet>> context)
    {
        Guard.AgainstNull(context);

        var request = context.Message.Request;

        var permission = new Permission();
        var stream = await _eventStore.GetAsync(request.Id);

        stream.Apply(permission);

        if (permission.Name.Equals(request.Name))
        {
            return;
        }

        var key = Permission.Key(permission.Name);
        var rekey = Permission.Key(request.Name);

        if (await _idKeyRepository.ContainsAsync(rekey) || !await _idKeyRepository.ContainsAsync(key))
        {
            return;
        }

        await _idKeyRepository.RekeyAsync(key, rekey);

        stream.Add(permission.SetName(request.Name));

        context.Message.WithResponse(new()
        {
            Id = request.Id,
            Name = request.Name,
            SequenceNumber = await _eventStore.SaveAsync(stream)
        });
    }
}