using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterTenantParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RegisterTenant>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);

    public async Task HandleAsync(RegisterTenant message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var key = Tenant.Key(message.Name);

        if (await _idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        var id = message.Id!.Value;

        await _idKeyRepository.AddAsync(id, key, cancellationToken);

        var aggregate = new Tenant();
        var stream = await _eventStore.GetAsync(id, cancellationToken);

        stream.Add(aggregate.Register(message.Name, message.Status, message.LogoSvg, message.LogoUrl));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}