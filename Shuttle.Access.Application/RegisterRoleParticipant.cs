using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterRoleParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository, IPermissionQuery permissionQuery)
    : IParticipant<RequestResponseMessage<RegisterRole, RoleRegistered>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);
    private readonly IPermissionQuery _permissionQuery = Guard.AgainstNull(permissionQuery);

    public async Task ProcessMessageAsync(RequestResponseMessage<RegisterRole, RoleRegistered> context, CancellationToken cancellationToken = default)
    {
        var request = Guard.AgainstNull(context).Request;

        var permissionIds = new List<Guid>();

        foreach (var permission in request.GetPermissions())
        {
            var permissionId = (await _permissionQuery.SearchAsync(new PermissionSpecification().AddName(permission.Name), cancellationToken)).FirstOrDefault()?.Id;

            if (permissionId.HasValue)
            {
                permissionIds.Add(permissionId.Value);
            }
            else
            {
                request.MissingPermissions();
                return;
            }
        }

        var key = Role.Key(request.Name);

        if (await _idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        var id = Guid.NewGuid();

        await _idKeyRepository.AddAsync(id, key, cancellationToken);

        var role = new Role();
        var stream = await _eventStore.GetAsync(id, cancellationToken);

        stream.Add(role.Register(request.AuditInformation.TenantId, request.Name));

        foreach (var permissionId in permissionIds)
        {
            if (!role.HasPermission(permissionId))
            {
                stream.Add(role.AddPermission(permissionId));
            }
        }

        await _eventStore.SaveAsync(stream, builder => builder.Audit(request.AuditInformation), cancellationToken);

        context.WithResponse(new()
        {
            Id = id,
            Name = request.Name
        });
    }
}