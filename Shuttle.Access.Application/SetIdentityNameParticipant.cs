using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class SetIdentityNameParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RequestResponseMessage<SetIdentityName, IdentityNameSet>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);

    public async Task HandleAsync(RequestResponseMessage<SetIdentityName, IdentityNameSet> context, CancellationToken cancellationToken = default)
    {
        var request = Guard.AgainstNull(context).Request;

        var identity = new Identity();
        var stream = await _eventStore.GetAsync(request.Id, cancellationToken);

        stream.Apply(identity);

        if (identity.Name.Equals(request.Name))
        {
            return;
        }

        var key = Identity.Key(identity.Name);
        var rekey = Identity.Key(request.Name);

        if (await _idKeyRepository.ContainsAsync(rekey, cancellationToken) || !await _idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await _idKeyRepository.RekeyAsync(key, rekey, cancellationToken);

        stream.Add(identity.SetName(request.Name));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(request), cancellationToken);

        context.WithResponse(new()
        {
            Id = request.Id,
            Name = request.Name
        });
    }
}