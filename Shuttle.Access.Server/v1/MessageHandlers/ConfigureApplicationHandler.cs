using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Mediator;
using System.Transactions;
using Shuttle.Access.Application;
using RegisterIdentity = Shuttle.Access.Application.RegisterIdentity;
using RegisterPermission = Shuttle.Access.Application.RegisterPermission;
using RegisterRole = Shuttle.Access.Application.RegisterRole;
using RegisterTenant = Shuttle.Access.Application.RegisterTenant;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class ConfigureApplicationHandler(ILogger<ConfigureApplicationHandler> logger, IOptions<ServerOptions> serverOptions, IOptions<AccessOptions> accessOptions, IMediator mediator, ITenantQuery tenantQuery, IRoleQuery roleQuery, IPermissionQuery permissionQuery, IIdentityQuery identityQuery, IHashingService hashingService, IBus bus)
    : IContextMessageHandler<ConfigureApplication>
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

    public async Task HandleAsync(IHandlerContext<ConfigureApplication> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var transportMessage  = Guard.AgainstNull( context.State.GetTransportMessage());

        if (transportMessage.SentAt < (DateTimeOffset.UtcNow - TimeSpan.FromSeconds(30)))
        {
            logger.LogWarning($"Message 'ConfigureApplication' was sent at '{transportMessage.SentAt}'.  The message is too old and has been ignored.  Another message may have been sent at server startup; else re-start the server.");
            return;
        }

        var message = context.Message;

        var getEventSourcingCounts = new GetEventSourcingCounts();

        await mediator.SendAsync(getEventSourcingCounts, cancellationToken);

        if (getEventSourcingCounts.HasUnsequencedPrimitiveEvents || getEventSourcingCounts.HasWaitingProjections)
        {
            await bus.SendAsync(message, builder => builder.ToSelf().DeferFor(TimeSpan.FromSeconds(5)), cancellationToken);
            return;
        }

        var systemTenantId = accessOptions.Value.SystemTenantId;

        foreach (var permission in _permissions)
        {
            if (await permissionQuery.ContainsAsync(new Query.Permission.Specification().AddName(permission), cancellationToken))
            {
                continue;
            }

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            logger.LogDebug($"Registering permission '{permission}'.");

            var registerPermissionMessage = new RegisterPermission(Guid.NewGuid(), permission, string.Empty, PermissionStatus.Active, systemTenantId, "system");

            await mediator.SendAsync(registerPermissionMessage, cancellationToken);

            scope.Complete();
        }

        var permissionSpecification = new Query.Permission.Specification();

        foreach (var permission in _permissions)
        {
            permissionSpecification.AddName(permission);
        }

        var timeout = DateTimeOffset.Now.Add(serverOptions.Value.Timeout);

        while (await permissionQuery.CountAsync(permissionSpecification, cancellationToken) != _permissions.Count && DateTimeOffset.Now < timeout)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).WaitAsync(cancellationToken);
        }

        if (await permissionQuery.CountAsync(permissionSpecification, cancellationToken) != _permissions.Count)
        {
            throw new ApplicationException(Application.Resources.PermissionsException);
        }

        /*
         * System Tenant
         */
        var tenantSpecification = new Query.Tenant.Specification()
            .AddId(systemTenantId);

        var systemTenantExists = await tenantQuery.CountAsync(tenantSpecification, cancellationToken) > 0;

        if (systemTenantExists)
        {
            logger.LogInformation("Found system tenant '{SystemTenantName}' with id '{SystemTenantId}'.", accessOptions.Value.SystemTenantName, systemTenantId);
        }
        else
        {
            logger.LogInformation("Registering system tenant '{SystemTenantName}' with id '{SystemTenantId}'.", accessOptions.Value.SystemTenantName, systemTenantId);

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var registerTenant = new RegisterTenant(systemTenantId, accessOptions.Value.SystemTenantName, TenantStatus.Active, systemTenantId, "system");

                await mediator.SendAsync(registerTenant, cancellationToken);

                scope.Complete();
            }

            timeout = DateTimeOffset.Now.Add(serverOptions.Value.Timeout);

            while (await tenantQuery.CountAsync(tenantSpecification, cancellationToken) == 0 && DateTimeOffset.Now < timeout)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).WaitAsync(cancellationToken);
            }

            if (await tenantQuery.CountAsync(tenantSpecification, cancellationToken) == 0)
            {
                throw new ApplicationException(Application.Resources.SystemTenantException);
            }
        }

        /*
         * Administrator Role
         */
        var roleSpecification = new Query.Role.Specification()
            .WithTenantId(systemTenantId)
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
                throw new ApplicationException(Application.Resources.AdministratorPermissionException);
            }

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            await mediator.SendAsync(new RegisterRole(Guid.NewGuid(), systemTenantId, "Access Administrator", systemTenantId, "system").AddPermissionId(administratorPermission.Id), cancellationToken);

            scope.Complete();

            timeout = DateTimeOffset.Now.Add(serverOptions.Value.Timeout);

            while (await roleQuery.CountAsync(roleSpecification, cancellationToken) == 0 && DateTimeOffset.Now < timeout)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).WaitAsync(cancellationToken);
            }

            administratorRole = (await roleQuery.SearchAsync(roleSpecification, cancellationToken)).FirstOrDefault();

            if (administratorRole == null)
            {
                throw new ApplicationException(Application.Resources.AdministratorRoleException);
            }
        }

        var role = (await roleQuery.SearchAsync(roleSpecification, cancellationToken)).SingleOrDefault();

        if (role == null)
        {
            throw new ApplicationException(Application.Resources.AdministratorRoleException);
        }

        logger.LogDebug($"Registering system administrator with identity name '{accessOptions.Value.SystemAdministratorIdentityName}'.");

        var systemAdministrator = (await identityQuery.SearchAsync(new Query.Identity.Specification()
            .WithTenantId(systemTenantId)
            .WithName(accessOptions.Value.SystemAdministratorIdentityName), cancellationToken)).FirstOrDefault();

        var registerIdentityMessage = new RegisterIdentity(systemAdministrator?.Id ?? Guid.NewGuid(), accessOptions.Value.SystemAdministratorIdentityName, string.Empty, string.Empty, hashingService.Sha256(accessOptions.Value.SystemAdministratorPassword), "system://access", true, systemTenantId, "system")
            .AddTenantId(systemTenantId)
            .AddRoleId(administratorRole.Id);

        await mediator.SendAsync(registerIdentityMessage, cancellationToken);
    }
}