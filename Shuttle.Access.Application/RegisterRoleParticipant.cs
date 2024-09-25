using System;
using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application;

public class RegisterRoleParticipant : IAsyncParticipant<RequestResponseMessage<RegisterRole, RoleRegistered>>
{
    private readonly IEventStore _eventStore;
    private readonly IKeyStore _keyStore;

    public RegisterRoleParticipant(IEventStore eventStore, IKeyStore keyStore)
    {
        Guard.AgainstNull(eventStore, nameof(eventStore));
        Guard.AgainstNull(keyStore, nameof(keyStore));

        _eventStore = eventStore;
        _keyStore = keyStore;
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<RegisterRole, RoleRegistered>> context)
    {
        Guard.AgainstNull(context, nameof(context));

        var message = context.Message.Request;

        var key = Role.Key(message.Name);

        if (await _keyStore.ContainsAsync(key))
        {
            return;
        }

        var id = Guid.NewGuid();

        await _keyStore.AddAsync(id, key);

        var role = new Role();
        var stream = await _eventStore.GetAsync(id);

        stream.AddEvent(role.Register(message.Name));

        context.Message.WithResponse(new()
        {
            Id = id,
            Name = message.Name,
            SequenceNumber = await _eventStore.SaveAsync(stream)
        });
    }
}