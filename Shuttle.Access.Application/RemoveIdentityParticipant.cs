using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application;

public class RemoveIdentityParticipant : IParticipant<RemoveIdentity>
{
    private readonly IEventStore _eventStore;
    private readonly IIdKeyRepository _idKeyRepository;

    public RemoveIdentityParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository)
    {
        Guard.AgainstNull(eventStore);
        Guard.AgainstNull(idKeyRepository);

        _eventStore = eventStore;
        _idKeyRepository = idKeyRepository;
    }

    public async Task ProcessMessageAsync(IParticipantContext<RemoveIdentity> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message;
        var id = message.Id;
        var identity = new Identity();
        var stream = await _eventStore.GetAsync(id);

        stream.Apply(identity);

        stream.Add(identity.Remove());

        await _idKeyRepository.RemoveAsync(id);

        await _eventStore.SaveAsync(stream);
    }
}