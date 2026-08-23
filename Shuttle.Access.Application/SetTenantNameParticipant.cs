using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class SetTenantNameParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<SetTenantName>
{
    public async Task HandleAsync(SetTenantName message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var stream = (await eventStore.GetAsync(message.Id, cancellationToken)).MustHaveEvents();
        var tenant = stream.Get<Tenant>();

        if (tenant.Name.Equals(message.Name))
        {
            return;
        }

        var key = Tenant.Key(tenant.Name);
        var rekey = Tenant.Key(message.Name);

        if (await idKeyRepository.ContainsAsync(rekey, cancellationToken) || !await idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await idKeyRepository.RekeyAsync(key, rekey, cancellationToken);

        stream.Add(tenant.SetName(message.Name));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
