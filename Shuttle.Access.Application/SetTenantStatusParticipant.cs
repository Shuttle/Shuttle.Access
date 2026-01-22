using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetTenantStatusParticipant(IEventStore eventStore) : IParticipant<RequestResponseMessage<SetTenantStatus, TenantStatusSet>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task ProcessMessageAsync(RequestResponseMessage<SetTenantStatus, TenantStatusSet> message, CancellationToken cancellationToken = default)
    {
        var request = Guard.AgainstNull(message).Request;
        var stream = await _eventStore.GetAsync(request.Id, cancellationToken);

        if (stream.IsEmpty)
        {
            return;
        }

        var tenant = new Tenant();

        stream.Apply(tenant);

        if (tenant.Status == message.Request.Status)
        {
            return;
        }

        stream.Add(tenant.SetStatus(request.Status));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message.Request), cancellationToken);

        message.WithResponse(new()
        {
            Id = message.Request.Id,
            Name = tenant.Name,
            Status = message.Request.Status,
            Version = stream.Version
        });
    }
}
