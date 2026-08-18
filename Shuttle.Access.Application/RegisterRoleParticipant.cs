using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterRoleParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository, IPermissionQuery permissionQuery) : IParticipant<RegisterRole>
{
    private static readonly TimeSpan PermissionResolutionTimeout = TimeSpan.FromSeconds(25);

    public async Task HandleAsync(RegisterRole message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(eventStore);
        ArgumentNullException.ThrowIfNull(idKeyRepository);

        var permissionIds = new List<Guid>(message.PermissionIds);

        foreach (var permissionName in message.PermissionNames)
        {
            var permissionId = await ResolvePermissionIdAsync(permissionName, cancellationToken);

            if (!permissionIds.Contains(permissionId))
            {
                permissionIds.Add(permissionId);
            }
        }

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

        foreach (var permissionId in permissionIds)
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

    private async Task<Guid> ResolvePermissionIdAsync(string permissionName, CancellationToken cancellationToken)
    {
        var timeout = DateTimeOffset.Now.Add(PermissionResolutionTimeout);

        Guid? permissionId;

        do
        {
            permissionId = (await permissionQuery.SearchAsync(new Query.Permission.Specification().AddName(permissionName), cancellationToken)).FirstOrDefault()?.Id;

            if (!permissionId.HasValue)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            }
        } while (!permissionId.HasValue && DateTimeOffset.Now < timeout);

        return permissionId ?? throw new ApplicationException($"Could not find a permission named '{permissionName}'.");
    }
}
