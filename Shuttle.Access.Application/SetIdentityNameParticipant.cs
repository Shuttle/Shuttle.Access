using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application;

public class SetIdentityNameParticipant : IAsyncParticipant<RequestResponseMessage<SetIdentityName, IdentityNameSet>>
{
    private readonly IEventStore _eventStore;
    private readonly IKeyStore _keyStore;

    public SetIdentityNameParticipant(IEventStore eventStore, IKeyStore keyStore)
    {
        Guard.AgainstNull(eventStore, nameof(eventStore));
        Guard.AgainstNull(keyStore, nameof(keyStore));

        _eventStore = eventStore;
        _keyStore = keyStore;
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<SetIdentityName, IdentityNameSet>> context)
    {
        Guard.AgainstNull(context, nameof(context));

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

        if (await _keyStore.ContainsAsync(rekey) || !await _keyStore.ContainsAsync(key))
        {
            return;
        }

        await _keyStore.RekeyAsync(key, rekey);

        stream.AddEvent(identity.SetName(request.Name));

        context.Message.WithResponse(new()
        {
            Id = request.Id,
            Name = request.Name,
            SequenceNumber = await _eventStore.SaveAsync(stream)
        });
    }
}