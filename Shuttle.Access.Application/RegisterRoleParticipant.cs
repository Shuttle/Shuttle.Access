using Shuttle.Access.SqlServer;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterRoleParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository, IPermissionQuery permissionQuery)
    : IParticipant<RegisterRole>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);
    private readonly IPermissionQuery _permissionQuery = Guard.AgainstNull(permissionQuery);

    public async Task HandleAsync(RegisterRole message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var permissionIds = new List<Guid>();

        foreach (var permission in message.GetPermissions())
        {
            var permissionId = (await _permissionQuery.SearchAsync(new PermissionSpecification().AddName(permission.Name), cancellationToken)).FirstOrDefault()?.Id;

            if (permissionId.HasValue)
            {
                permissionIds.Add(permissionId.Value);
            }
            else
            {
                message.MissingPermissions();
                return;
            }
        }

        var key = Role.Key(message.Name, message.TenantId);

        if (await _idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await _idKeyRepository.AddAsync(message.Id, key, cancellationToken);

        var role = new Role();
        var stream = await _eventStore.GetAsync(message.Id, cancellationToken);

        stream.Add(role.Register(message.TenantId, message.Name));

        foreach (var permissionId in permissionIds)
        {
            if (!role.HasPermission(permissionId))
            {
                stream.Add(role.AddPermission(permissionId));
            }
        }

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message.AuditInformation), cancellationToken);
    }
}