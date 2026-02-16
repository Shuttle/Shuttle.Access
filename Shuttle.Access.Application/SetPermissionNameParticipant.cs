using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class SetPermissionNameParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RequestResponseMessage<SetPermissionName, PermissionNameSet>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);

    public async Task HandleAsync(RequestResponseMessage<SetPermissionName, PermissionNameSet> context, CancellationToken cancellationToken = default)
    {
        var request = Guard.AgainstNull(context).Request;

        var permission = new Permission();
        var stream = await _eventStore.GetAsync(request.Id, cancellationToken);

        stream.Apply(permission);

        if (permission.Name.Equals(request.Name))
        {
            return;
        }

        var key = Permission.Key(permission.Name);
        var rekey = Permission.Key(request.Name);

        if (await _idKeyRepository.ContainsAsync(rekey, cancellationToken) || !await _idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await _idKeyRepository.RekeyAsync(key, rekey, cancellationToken);

        stream.Add(permission.SetName(request.Name));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(request), cancellationToken);

        context.WithResponse(new()
        {
            Id = request.Id,
            Name = request.Name
        });
    }
}