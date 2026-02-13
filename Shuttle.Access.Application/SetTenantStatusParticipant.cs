using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetTenantStatusParticipant(IEventStore eventStore) : IParticipant<RequestResponseMessage<SetTenantStatus, TenantStatusSet>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task ProcessMessageAsync(RequestResponseMessage<SetTenantStatus, TenantStatusSet> context, CancellationToken cancellationToken = default)
    {
        var request = Guard.AgainstNull(context
        
        ).Request;
        var stream = await _eventStore.GetAsync(request.Id, cancellationToken);

        if (stream.IsEmpty)
        {
            return;
        }

        var tenant = new Tenant();

        stream.Apply(tenant);

        if (tenant.Status == context.Request.Status)
        {
            return;
        }

        stream.Add(tenant.SetStatus(request.Status));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(context.Request), cancellationToken);

        context.WithResponse(new()
        {
            Id = context.Request.Id,
            Name = tenant.Name,
            Status = context.Request.Status,
            Version = stream.Version
        });
    }
}
