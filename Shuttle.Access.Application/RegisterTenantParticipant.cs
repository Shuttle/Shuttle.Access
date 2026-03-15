using Shuttle.Core.Mediator;
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

        if (await idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await idKeyRepository.AddAsync(message.Id, key, cancellationToken);

        var stream = (await eventStore.GetAsync(message.Id, cancellationToken)).MustBeEmpty();
        var aggregate = stream.Get<Tenant>();

        stream.Add(aggregate.Register(message.Name, (int)message.Status, message.LogoSvg, message.LogoUrl));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}