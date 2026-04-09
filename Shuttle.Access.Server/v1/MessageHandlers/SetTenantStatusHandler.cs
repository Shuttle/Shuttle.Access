using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetTenantStatusHandler(IEventStore eventStore) :
    IMessageHandler<SetTenantStatus>
{
    public async Task HandleAsync(SetTenantStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var stream = (await eventStore.GetAsync(message.Id, cancellationToken)).MustHaveEvents();
        var aggregate = stream.Get<Tenant>();

        var status = (TenantStatus)message.Status;

        if (aggregate.Status == status)
        {
            return;
        }

        stream.Add(aggregate.SetStatus(status));

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}