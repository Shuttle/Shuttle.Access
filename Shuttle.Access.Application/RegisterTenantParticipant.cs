using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterTenantParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository, IMediator mediator, ITenantQuery tenantQuery, IRoleQuery roleQuery, IPermissionQuery permissionQuery, IIdentityQuery identityQuery)
    : IParticipant<RegisterTenant>
{
    private static readonly TimeSpan ProjectionWaitTimeout = TimeSpan.FromSeconds(15);

    public async Task HandleAsync(RegisterTenant message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(eventStore);
        ArgumentNullException.ThrowIfNull(idKeyRepository);

        var key = Tenant.Key(message.Name);
        var id = await idKeyRepository.FindAsync(key, cancellationToken);

        if (!id.HasValue)
        {
            await idKeyRepository.AddAsync(message.Id, key, cancellationToken);
        }
        else
        {
            if (!id.Value.Equals(message.Id))
            {
                throw new ApplicationException($"There is already a tenant key '{key}' which is associated with id '{id.Value}'.");
            }
        }

        var stream = await eventStore.GetAsync(message.Id, cancellationToken);
        var aggregate = stream.Get<Tenant>();

        if (string.IsNullOrWhiteSpace(aggregate.Name))
        {
            stream.Add(aggregate.Register(message.Name, (int)message.Status, message.LogoSvg, message.LogoUrl, message.MaximumIdentities));

            await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(message.AdministratorIdentityName))
        {
            return;
        }

        var identity = (await identityQuery.SearchAsync(new Query.Identity.Specification().WithName(message.AdministratorIdentityName), cancellationToken)).FirstOrDefault();

        if (identity == null)
        {
            throw new ApplicationException($"Could not find the administrator identity with name '{message.AdministratorIdentityName}'.");
        }

        var accessAdministratorPermission = (await permissionQuery.SearchAsync(new Query.Permission.Specification().AddName(AccessPermissions.Administrator), cancellationToken)).FirstOrDefault();

        if (accessAdministratorPermission == null)
        {
            throw new ApplicationException($"Could not find the Access administrator permission '{AccessPermissions.Administrator}'.");
        }

        Query.Tenant? tenant;
        var timeout = DateTimeOffset.Now.Add(ProjectionWaitTimeout);

        do
        {
            tenant = (await tenantQuery.SearchAsync(new Query.Tenant.Specification().AddId(message.Id), cancellationToken)).FirstOrDefault();

            if (tenant == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        } while (tenant == null && DateTimeOffset.Now < timeout);

        if (tenant == null)
        {
            throw new ApplicationException($"Timed out waiting for tenant '{message.Name}' to be registered.");
        }

        var registerRoleMessage = new RegisterRole(message.AccessAdministratorRoleId, message.Id, "Access Administrator", message.AuditTenantId, message.AuditIdentityName)
            .AddPermissionId(accessAdministratorPermission.Id);

        await mediator.SendAsync(registerRoleMessage, cancellationToken);

        Query.Role? role;

        timeout = DateTimeOffset.Now.Add(ProjectionWaitTimeout);

        do
        {
            role = (await roleQuery.SearchAsync(new Query.Role.Specification().AddId(message.AccessAdministratorRoleId), cancellationToken)).FirstOrDefault();

            if (role == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        } while (role == null && DateTimeOffset.Now < timeout);

        if (role == null)
        {
            throw new ApplicationException("Timed out waiting for role 'Access Administrator' to be registered.");
        }

        await mediator.SendAsync(new SetIdentityTenantStatus(identity.Id, message.Id, true, message.Id, "system"), cancellationToken);
        await mediator.SendAsync(new SetIdentityRoleStatus(identity.Id, role.Id, true, message.Id, "system"), cancellationToken);
    }
}
