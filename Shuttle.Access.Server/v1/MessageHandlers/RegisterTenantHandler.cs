using Shuttle.Access.Messages.v1;
using Shuttle.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RegisterTenantHandler(ITenantQuery tenantQuery, IRoleQuery roleQuery, IPermissionQuery permissionQuery, IIdentityQuery identityQuery, IMediator mediator, IBus bus) :  IMessageHandler<RegisterTenant>
{
    public async Task HandleAsync(RegisterTenant message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenantQuery);
        ArgumentNullException.ThrowIfNull(roleQuery);
        ArgumentNullException.ThrowIfNull(permissionQuery);
        ArgumentNullException.ThrowIfNull(identityQuery);
        ArgumentNullException.ThrowIfNull(mediator);
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(message);

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

        var registerTenant = new Application.RegisterTenant(message.Id, message.Name, (TenantStatus)message.Status, message.AuditTenantId, message.AuditIdentityName)
        {
            LogoUrl = message.LogoUrl,
            LogoSvg = message.LogoSvg
        };

        await mediator.SendAsync(registerTenant, cancellationToken);

        Query.Tenant? tenant;

        var timeout = DateTime.UtcNow.AddSeconds(15);

        do
        {
            tenant = (await tenantQuery.SearchAsync(new Query.Tenant.Specification().AddId(message.Id), cancellationToken)).FirstOrDefault();

            if (tenant == null)
            {
                await Task.Delay(1000, cancellationToken);
            }
        } while (tenant == null && DateTime.UtcNow < timeout);

        if (tenant == null)
        {
            throw new ApplicationException($"Timed out waiting for tenant '{message.Name}' to be registered.");
        }

        var auditInformation = new AuditInformation(message.Id, "system");

        var registerRoleMessage = new Application.RegisterRole(message.AccessAdministratorRoleId, message.Id, "Access Administrator", message.AuditTenantId, message.AuditIdentityName);

        registerRoleMessage.AddPermissionId(accessAdministratorPermission.Id);

        await mediator.SendAsync(registerRoleMessage, cancellationToken);

        Query.Role? role;

        timeout = DateTime.UtcNow.AddSeconds(15);
        do
        {
            role = (await roleQuery.SearchAsync(new Query.Role.Specification().AddId(message.AccessAdministratorRoleId), cancellationToken)).FirstOrDefault();

            if (role == null)
            {
                await Task.Delay(1000, cancellationToken);
            }
        } while (role == null && DateTime.UtcNow < timeout);

        if (role == null)
        {
            throw new ApplicationException("Timed out waiting for role 'Access Administrator' to be registered.");
        }

        var registerAdministratorMessage = new SetIdentityTenantStatus
        {
            TenantId = message.Id,
            IdentityId = identity.Id,
            Active = true,
            AuditIdentityName = auditInformation.AuditIdentityName,
            AuditTenantId = auditInformation.AuditTenantId
        };

        await bus.SendAsync(registerAdministratorMessage, builder => builder.ToSelf(), cancellationToken);

        var setIdentityRoleMessage = new SetIdentityRoleStatus
        {
            IdentityId = identity.Id,
            RoleId = role.Id,
            Active = true,
            AuditIdentityName = auditInformation.AuditIdentityName,
            AuditTenantId = auditInformation.AuditTenantId
        };

        await bus.SendAsync(setIdentityRoleMessage, builder => builder.ToSelf(), cancellationToken);
    }
}