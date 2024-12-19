using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application;

public class SetIdentityNameParticipant : IParticipant<RequestResponseMessage<SetIdentityName, IdentityNameSet>>
{
    private readonly IEventStore _eventStore;
    private readonly IIdKeyRepository _idKeyRepository;

    public SetIdentityNameParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository)
    {
        Guard.AgainstNull(eventStore);
        Guard.AgainstNull(idKeyRepository);

        _eventStore = eventStore;
        _idKeyRepository = idKeyRepository;
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<SetIdentityName, IdentityNameSet>> context)
    {
        Guard.AgainstNull(context);

        var request = context.Message.Request;

        var identity = new Identity();
        var stream = await _eventStore.GetAsync(request.Id);

        stream.Apply(identity);

        if (identity.Name.Equals(request.Name))
        {
            return;
        }

        var key = Identity.Key(identity.Name);
        var rekey = Identity.Key(request.Name);

        if (await _idKeyRepository.ContainsAsync(rekey) || !await _idKeyRepository.ContainsAsync(key))
        {
            return;
        }

        await _idKeyRepository.RekeyAsync(key, rekey);

        stream.Add(identity.SetName(request.Name));

        context.Message.WithResponse(new()
        {
            Id = request.Id,
            Name = request.Name,
            SequenceNumber = await _eventStore.SaveAsync(stream)
        });
    }
}