using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class SetPermissionNameParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<SetPermissionName>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);

    public async Task HandleAsync(SetPermissionName message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var permission = new Permission();
        var stream = await _eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(permission);

        if (permission.Name.Equals(message.Name))
        {
            return;
        }

        var key = Permission.Key(permission.Name);
        var rekey = Permission.Key(message.Name);

        if (await _idKeyRepository.ContainsAsync(rekey, cancellationToken) || !await _idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await _idKeyRepository.RekeyAsync(key, rekey, cancellationToken);

        stream.Add(permission.SetName(message.Name));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}