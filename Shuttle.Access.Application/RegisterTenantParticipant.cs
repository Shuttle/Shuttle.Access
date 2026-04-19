using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterTenantParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RegisterTenant>
{
    public async Task HandleAsync(RegisterTenant message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(eventStore);
        ArgumentNullException.ThrowIfNull(idKeyRepository);

        var key = Tenant.Key(message.Name);
        var id = await idKeyRepository.FindAsync(key, cancellationToken);

        if (!id.HasValue)
        {
            await idKeyRepository.AddAsync(message.Id, key, cancellationToken);
        }
        else
        {
            if (!id.Value.Equals(message.Id))
            {
                throw new ApplicationException($"There is already a tenant key '{key}' which is associated with id '{id.Value}'.");
            }
        }
        
        var stream = await eventStore.GetAsync(message.Id, cancellationToken);
        var aggregate = stream.Get<Tenant>();

        if (!string.IsNullOrWhiteSpace(aggregate.Name))
        {
            return;
        }

        stream.Add(aggregate.Register(message.Name, (int)message.Status, message.LogoSvg, message.LogoUrl));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}