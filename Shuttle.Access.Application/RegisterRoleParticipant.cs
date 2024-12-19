using System;
using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application;

public class RegisterRoleParticipant : IParticipant<RequestResponseMessage<RegisterRole, RoleRegistered>>
{
    private readonly IEventStore _eventStore;
    private readonly IIdKeyRepository _idKeyRepository;

    public RegisterRoleParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository)
    {
        Guard.AgainstNull(eventStore);
        Guard.AgainstNull(idKeyRepository);

        _eventStore = eventStore;
        _idKeyRepository = idKeyRepository;
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<RegisterRole, RoleRegistered>> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message.Request;

        var key = Role.Key(message.Name);

        if (await _idKeyRepository.ContainsAsync(key))
        {
            return;
        }

        var id = Guid.NewGuid();

        await _idKeyRepository.AddAsync(id, key);

        var role = new Role();
        var stream = await _eventStore.GetAsync(id);

        stream.Add(role.Register(message.Name));

        context.Message.WithResponse(new()
        {
            Id = id,
            Name = message.Name,
            SequenceNumber = await _eventStore.SaveAsync(stream)
        });
    }
}