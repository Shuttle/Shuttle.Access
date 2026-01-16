using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterPermissionParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RequestResponseMessage<RegisterPermission, PermissionRegistered>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);

    public async Task ProcessMessageAsync(RequestResponseMessage<RegisterPermission, PermissionRegistered> message, CancellationToken cancellationToken = default)
    {
        var request = Guard.AgainstNull(message).Request;

        var key = Permission.Key(request.Name);

        if (await _idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        var id = Guid.NewGuid();

        await _idKeyRepository.AddAsync(id, key, cancellationToken);

        var aggregate = new Permission();
        var stream = await _eventStore.GetAsync(id, cancellationToken);
        var status = request.Status;

        if (!Enum.IsDefined(typeof(PermissionStatus), status))
        {
            status = (int)PermissionStatus.Active;
        }

        stream.Add(aggregate.Register(request.Name, request.Description, (PermissionStatus)status));

        await _eventStore.SaveAsync(stream, builder => builder.AddAuditIdentityName(request.AuditIdentityName), cancellationToken);

        message.WithResponse(new()
        {
            Id = id,
            Name = request.Name,
            Description = request.Description
        });
    }
}