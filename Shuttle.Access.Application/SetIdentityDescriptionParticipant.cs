using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application;

public class SetIdentityDescriptionParticipant : IParticipant<RequestResponseMessage<SetIdentityDescription, IdentityDescriptionSet>>
{
    private readonly IEventStore _eventStore;
    private readonly IIdKeyRepository _idKeyRepository;

    public SetIdentityDescriptionParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository)
    {
        _eventStore = Guard.AgainstNull(eventStore);
        _idKeyRepository = Guard.AgainstNull(idKeyRepository);
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<SetIdentityDescription, IdentityDescriptionSet>> context)
    {
        Guard.AgainstNull(context);

        var request = context.Message.Request;

        var identity = new Identity();
        var stream = await _eventStore.GetAsync(request.Id);

        stream.Apply(identity);

        if (identity.Description.Equals(request.Description))
        {
            return;
        }

        stream.Add(identity.SetDescription(request.Description));

        await _eventStore.SaveAsync(stream);

        context.Message.WithResponse(new()
        {
            Id = request.Id,
            Description = request.Description
        });
    }
}