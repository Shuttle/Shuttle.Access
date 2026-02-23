using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterPermissionParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RegisterPermission>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);

    public async Task HandleAsync(RegisterPermission message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var key = Permission.Key(message.Name);

        if (await _idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        var id = Guid.NewGuid();

        await _idKeyRepository.AddAsync(id, key, cancellationToken);

        var aggregate = new Permission();
        var stream = await _eventStore.GetAsync(id, cancellationToken);
        var status = message.Status;

        if (!Enum.IsDefined(typeof(PermissionStatus), status))
        {
            status = (int)PermissionStatus.Active;
        }

        stream.Add(aggregate.Register(message.Name, message.Description, (PermissionStatus)status));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}