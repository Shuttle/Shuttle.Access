using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application;

public class SetRoleNameParticipant : IParticipant<RequestResponseMessage<SetRoleName, RoleNameSet>>
{
    private readonly IEventStore _eventStore;
    private readonly IIdKeyRepository _idKeyRepository;

    public SetRoleNameParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository)
    {
        Guard.AgainstNull(eventStore);
        Guard.AgainstNull(idKeyRepository);

        _eventStore = eventStore;
        _idKeyRepository = idKeyRepository;
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<SetRoleName, RoleNameSet>> context)
    {
        Guard.AgainstNull(context);

        var request = context.Message.Request;

        var role = new Role();
        var stream = await _eventStore.GetAsync(request.Id);

        stream.Apply(role);

        if (role.Name.Equals(request.Name))
        {
            return;
        }

        var key = Role.Key(role.Name);
        var rekey = Role.Key(request.Name);

        if (await _idKeyRepository.ContainsAsync(rekey) || !await _idKeyRepository.ContainsAsync(key))
        {
            return;
        }

        await _idKeyRepository.RekeyAsync(key, rekey);

        stream.Add(role.SetName(request.Name));

        context.Message.WithResponse(new()
        {
            Id = request.Id,
            Name = request.Name,
            SequenceNumber = await _eventStore.SaveAsync(stream)
        });
    }
}