using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterRoleParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RegisterRole>
{
    public async Task HandleAsync(RegisterRole message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(eventStore);
        ArgumentNullException.ThrowIfNull(idKeyRepository);

        var key = Role.Key(message.Name, message.TenantId);

        if (await idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await idKeyRepository.AddAsync(message.Id, key, cancellationToken);

        var stream = (await eventStore.GetAsync(message.Id, cancellationToken)).MustBeEmpty();
        var aggregate = stream.Get<Role>();

        stream.Add(aggregate.Register(message.TenantId, message.Name));

        foreach (var permissionId in message.PermissionIds)
        {
            if (!aggregate.HasPermission(permissionId))
            {
                stream.Add(aggregate.AddPermission(permissionId));
            }
        }

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}