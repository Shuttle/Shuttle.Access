using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class SetRoleNameParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<SetRoleName>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);

    public async Task HandleAsync(SetRoleName message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var role = new Role();
        var stream = await _eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(role);

        if (role.Name.Equals(message.Name))
        {
            return;
        }

        var key = Role.Key(role.Name, role.TenantId);
        var rekey = Role.Key(message.Name, role.TenantId);

        if (await _idKeyRepository.ContainsAsync(rekey, cancellationToken) || !await _idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await _idKeyRepository.RekeyAsync(key, rekey, cancellationToken);

        stream.Add(role.SetName(message.Name));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}