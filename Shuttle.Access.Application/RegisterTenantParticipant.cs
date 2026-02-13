using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterTenantParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RequestResponseMessage<RegisterTenant, TenantRegistered>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);

    public async Task ProcessMessageAsync(RequestResponseMessage<RegisterTenant, TenantRegistered> context, CancellationToken cancellationToken = default)
    {
        var request = Guard.AgainstNull(context).Request;

        var key = Tenant.Key(request.Name);

        if (await _idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        var id = request.Id ?? Guid.NewGuid();

        await _idKeyRepository.AddAsync(id, key, cancellationToken);

        var aggregate = new Tenant();
        var stream = await _eventStore.GetAsync(id, cancellationToken);

        stream.Add(aggregate.Register(request.Name, request.Status, request.LogoSvg, request.LogoUrl));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(request), cancellationToken);

        context.WithResponse(new()
        {
            Id = id,
            Name = request.Name
        });
    }
}