using Shuttle.Mediator;
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
        var id = await idKeyRepository.FindAsync(key, cancellationToken);

        if (!id.HasValue)
        {
            await idKeyRepository.AddAsync(message.Id, key, cancellationToken);
        }
        else
        {
            if (!id.Value.Equals(message.Id))
            {
                throw new ApplicationException($"There is already a role key '{key}' which is associated with id '{id.Value}'.");
            }
        }

        var stream = (await eventStore.GetAsync(message.Id, cancellationToken));
        var aggregate = stream.Get<Role>();

        if (string.IsNullOrWhiteSpace(aggregate.Name))
        {
            stream.Add(aggregate.Register(message.TenantId, message.Name));
        }

        foreach (var permissionId in message.PermissionIds)
        {
            if (!aggregate.HasPermission(permissionId))
            {
                stream.Add(aggregate.AddPermission(permissionId));
            }
        }

        if (stream.ShouldSave())
        {
            await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
        }
    }
}