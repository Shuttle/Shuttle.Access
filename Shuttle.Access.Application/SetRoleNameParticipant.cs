using Microsoft.Extensions.Options;
using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class SetRoleNameParticipant(IOptions<AccessOptions> accessOptions, IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<SetRoleName>
{
    public async Task HandleAsync(SetRoleName message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Guard.AgainstNull(message).Name))
        {
            return;
        }

        var role = new Role();
        var stream = await eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(role);

        if (role.Name.Equals(message.Name))
        {
            return;
        }

        var tenantId = role.TenantId == Guid.Empty ? Guard.AgainstNull(accessOptions).Value.SystemTenantId : role.TenantId;

        var key = Role.Key(role.Name, tenantId);
        var rekey = Role.Key(message.Name, tenantId);

        if (await idKeyRepository.ContainsAsync(rekey, cancellationToken) || !await idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await idKeyRepository.RekeyAsync(key, rekey, cancellationToken);

        stream.Add(role.SetName(message.Name));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
