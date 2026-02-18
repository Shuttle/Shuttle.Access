using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetIdentityTenantStatusParticipant(IEventStore eventStore) : IParticipant<RequestResponseMessage<SetIdentityTenantStatus, IdentityTenantSet>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task HandleAsync(RequestResponseMessage<SetIdentityTenantStatus, IdentityTenantSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var identity = new Identity();
        var request = context.Request;
        var stream = await _eventStore.GetAsync(request.IdentityId, cancellationToken);

        stream.Apply(identity);

        if (request.Active && !identity.IsInTenant(request.TenantId))
        {
            stream.Add(identity.AddTenant(request.TenantId));
        }

        if (!request.Active && identity.IsInTenant(request.TenantId))
        {
            stream.Add(identity.RemoveTenant(request.TenantId));
        }

        if (stream.ShouldSave())
        {
            await _eventStore.SaveAsync(stream, builder => builder.Audit(context.Request), cancellationToken);

            context.WithResponse(new()
            {
                TenantId = request.TenantId,
                IdentityId = request.IdentityId,
                Active = request.Active,
                Version = stream.Version
            });
        }
    }
}