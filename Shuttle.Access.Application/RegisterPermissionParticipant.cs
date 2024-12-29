using System;
using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application;

public class RegisterPermissionParticipant : IParticipant<RequestResponseMessage<RegisterPermission, PermissionRegistered>>
{
    private readonly IEventStore _eventStore;
    private readonly IIdKeyRepository _idKeyRepository;

    public RegisterPermissionParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository)
    {
        _eventStore = Guard.AgainstNull(eventStore);
        _idKeyRepository = Guard.AgainstNull(idKeyRepository);
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<RegisterPermission, PermissionRegistered>> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message.Request;

        var key = Permission.Key(message.Name);

        if (await _idKeyRepository.ContainsAsync(key))
        {
            return;
        }

        var id = Guid.NewGuid();

        await _idKeyRepository.AddAsync(id, key);

        var aggregate = new Permission();
        var stream = await _eventStore.GetAsync(id);
        var status = message.Status;

        if (!Enum.IsDefined(typeof(PermissionStatus), status))
        {
            status = (int)PermissionStatus.Active;
        }

        stream.Add(aggregate.Register(message.Name, (PermissionStatus)status));

        context.Message.WithResponse(new()
        {
            Id = id,
            Name = message.Name,
            SequenceNumber = await _eventStore.SaveAsync(stream)
        });
    }
}