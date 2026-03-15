using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Access.Application;

public class ConfigureApplicationParticipant(ILogger<ConfigureApplicationParticipant> logger, IOptions<AccessOptions> accessOptions, ITransactionScopeFactory transactionScopeFactory, IMediator mediator, ITenantQuery tenantQuery, IRoleQuery roleQuery, IPermissionQuery permissionQuery, IIdentityQuery identityQuery, IHashingService hashingService)
    : IParticipant<ConfigureApplication>
{
    private readonly List<string> _permissions =
    [
        AccessPermissions.Administrator,
        AccessPermissions.Identities.Activate,
        AccessPermissions.Identities.Manage,
        AccessPermissions.Identities.Register,
        AccessPermissions.Identities.Remove,
        AccessPermissions.Identities.View,
        AccessPermissions.IdentityTenants.View,
        AccessPermissions.IdentityTenants.Remove,
        AccessPermissions.IdentityTenants.Manage,
        AccessPermissions.Roles.View,
        AccessPermissions.Roles.Register,
        AccessPermissions.Roles.Remove,
        AccessPermissions.Permissions.Manage,
        AccessPermissions.Permissions.Register,
        AccessPermissions.Permissions.View,
        AccessPermissions.Sessions.Manage,
        AccessPermissions.Sessions.Register,
        AccessPermissions.Sessions.View,
        AccessPermissions.Tenants.View,
        AccessPermissions.Tenants.Register,
        AccessPermissions.Tenants.Manage
    ];

    public async Task HandleAsync(ConfigureApplication message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        foreach (var permission in _permissions)
        {
            using (var scope = transactionScopeFactory.Create())
            {
                if (await permissionQuery.ContainsAsync(new Query.Permission.Specification().AddName(permission), cancellationToken))
                {
                    continue;
                }

                logger.LogDebug($"Registering permission '{permission}'.");

                var registerPermissionMessage = new RegisterPermission(Guid.NewGuid(), permission, string.Empty, PermissionStatus.Active, accessOptions.Value.SystemTenantId, "system");

                await mediator.SendAsync(registerPermissionMessage, cancellationToken);

                scope.Complete();
            }
        }

        var timeout = DateTimeOffset.Now.Add(message.Timeout);

        var permissionSpecification = new Query.Permission.Specification();

        foreach (var permission in _permissions)
        {
            permissionSpecification.AddName(permission);
        }

        while (await permissionQuery.CountAsync(permissionSpecification, cancellationToken) != _permissions.Count && DateTimeOffset.Now < timeout)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).WaitAsync(cancellationToken);
        }

        if (await permissionQuery.CountAsync(permissionSpecification, cancellationToken) != _permissions.Count)
        {
            throw new ApplicationException(Resources.PermissionsException);
        }

        if (!message.ShouldConfigure)
        {
            return;
        }

        /*
         * System Tenant
         */
        var tenantSpecification = new Query.Tenant.Specification()
            .AddId(accessOptions.Value.SystemTenantId);

        var systemTenantExists = await tenantQuery.CountAsync(tenantSpecification, cancellationToken) > 0;

        if (systemTenantExists)
        {
            logger.LogInformation("Found system tenant '{SystemTenantName}' with id '{SystemTenantId}'.", accessOptions.Value.SystemTenantName, accessOptions.Value.SystemTenantId);
        }
        else
        {
            logger.LogInformation("Registering system tenant '{SystemTenantName}' with id '{SystemTenantId}'.", accessOptions.Value.SystemTenantName, accessOptions.Value.SystemTenantId);

            using (var scope = transactionScopeFactory.Create())
            {
                var registerTenant = new RegisterTenant(accessOptions.Value.SystemTenantId, accessOptions.Value.SystemTenantName, TenantStatus.Active, accessOptions.Value.SystemTenantId, "system");

                await mediator.SendAsync(registerTenant, cancellationToken);

                scope.Complete();
            }

            if (await tenantQuery.CountAsync(tenantSpecification, cancellationToken) == 0)
            {
                throw new ApplicationException(Resources.SystemTenantException);
            }
        }

        /*
         * Administrator Role
         *
         */
        var roleSpecification = new Query.Role.Specification()
            .WithTenantId(accessOptions.Value.SystemTenantId)
            .AddName("Access Administrator");

        var administratorRole = (await roleQuery.SearchAsync(roleSpecification, cancellationToken)).FirstOrDefault();

        if (administratorRole != null)
        {
            logger.LogDebug("Found role 'Access Administrator' with id '{AdministratorRoleId}'.", administratorRole.Id);
        }
        else
        {
            logger.LogDebug("Registering role 'Access Administrator'.");

            var administratorPermission = (await permissionQuery.SearchAsync(new Query.Permission.Specification().AddName(AccessPermissions.Administrator), cancellationToken)).FirstOrDefault();

            if (administratorPermission == null)
            {
                throw new ApplicationException(Resources.AdministratorPermissionException);
            }

            using (var scope = transactionScopeFactory.Create())
            {
                await mediator.SendAsync(new RegisterRole(Guid.NewGuid(), accessOptions.Value.SystemTenantId, "Access Administrator", accessOptions.Value.SystemTenantId, "system").AddPermissionId(administratorPermission.Id), cancellationToken);

                scope.Complete();
            }

            timeout = DateTimeOffset.Now.Add(message.Timeout);

            while (await roleQuery.CountAsync(roleSpecification, cancellationToken) == 0 && DateTimeOffset.Now < timeout)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).WaitAsync(cancellationToken);
            }

            if (await roleQuery.CountAsync(roleSpecification, cancellationToken) == 0)
            {
                throw new ApplicationException(Resources.AdministratorRoleException);
            }
        }

        var role = (await roleQuery.SearchAsync(roleSpecification, cancellationToken)).SingleOrDefault();

        if (role == null)
        {
            throw new ApplicationException(Resources.AdministratorRoleException);
        }

        if (await identityQuery.CountAsync(new Query.Identity.Specification().WithRoleName("Access Administrator"), cancellationToken) == 0)
        {
            var registerIdentityMessage = new RegisterIdentity(Guid.NewGuid(), message.AdministratorIdentityName, string.Empty, string.Empty, hashingService.Sha256(message.AdministratorPassword), "system://access", true, accessOptions.Value.SystemTenantId, "system");

            await mediator.SendAsync(registerIdentityMessage, cancellationToken);
        }
    }
}