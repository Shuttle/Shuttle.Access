using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetIdentityTenantStatusHandler(IEventStore eventStore) : IMessageHandler<SetIdentityTenantStatus>
{
    public async Task HandleAsync(SetIdentityTenantStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var identity = new Identity();
        var stream = await eventStore.GetAsync(message.IdentityId, cancellationToken);

        stream.Apply(identity);

        if (message.Active && !identity.IsInTenant(message.TenantId))
        {
            stream.Add(identity.AddTenant(message.TenantId));
        }

        if (!message.Active && identity.IsInTenant(message.TenantId))
        {
            stream.Add(identity.RemoveTenant(message.TenantId));
        }

        if (stream.ShouldSave())
        {
            await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
        }
    }
}