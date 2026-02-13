using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RemoveRoleParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RequestResponseMessage<RemoveRole, RoleRemoved>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);

    public async Task ProcessMessageAsync(RequestResponseMessage<RemoveRole, RoleRemoved> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var stream = await _eventStore.GetAsync(context.Request.Id, cancellationToken: cancellationToken);

        if (stream.IsEmpty)
        {
            return;
        }

        var role = new Role();

        stream.Apply(role);

        stream.Add(role.Remove());

        await _idKeyRepository.RemoveAsync(context.Request.Id, cancellationToken);

        await _eventStore.SaveAsync(stream, builder => builder.Audit(context.Request), cancellationToken);

        context.WithResponse(new()
        {
            Id = context.Request.Id,
            Name = role.Name
        });
    }
}