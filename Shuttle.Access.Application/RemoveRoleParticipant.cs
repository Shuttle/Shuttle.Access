using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application;

public class RemoveRoleParticipant : IParticipant<RequestResponseMessage<RemoveRole, RoleRemoved>>
{
    private readonly IEventStore _eventStore;
    private readonly IIdKeyRepository _idKeyRepository;

    public RemoveRoleParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository)
    {
        Guard.AgainstNull(eventStore);
        Guard.AgainstNull(idKeyRepository);

        _eventStore = eventStore;
        _idKeyRepository = idKeyRepository;
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<RemoveRole, RoleRemoved>> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message;

        var stream = await _eventStore.GetAsync(message.Request.Id);

        if (stream.IsEmpty)
        {
            return;
        }

        var role = new Role();

        stream.Apply(role);

        stream.Add(role.Remove());

        await _idKeyRepository.RemoveAsync(message.Request.Id);

        await _eventStore.SaveAsync(stream);

        context.Message.WithResponse(new()
        {
            Id = message.Request.Id,
            Name = role.Name,
            SequenceNumber = await _eventStore.SaveAsync(stream)
        });
    }
}