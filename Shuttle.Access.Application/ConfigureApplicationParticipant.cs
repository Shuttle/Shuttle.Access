using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Access.Application;

public class ConfigureApplicationParticipant(ILogger<ConfigureApplicationParticipant> logger, IOptions<AccessOptions> accessOptions, ITransactionScopeFactory transactionScopeFactory, IMediator mediator, ITenantQuery tenantQuery, IRoleQuery roleQuery, IPermissionQuery permissionQuery, IIdentityQuery identityQuery, AccessDbContext accessDbContext)
    : IParticipant<ConfigureApplication>
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);
    private readonly AccessOptions _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly ILogger<ConfigureApplicationParticipant> _logger = Guard.AgainstNull(logger);
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);
    private readonly IPermissionQuery _permissionQuery = Guard.AgainstNull(permissionQuery);

    private readonly List<string> _permissions =
    [
        AccessPermissions.Identities.Activate,
        AccessPermissions.Identities.Manage,
        AccessPermissions.Identities.Register,
        AccessPermissions.Identities.Remove,
        AccessPermissions.Identities.View,
        AccessPermissions.Roles.View,
        AccessPermissions.Roles.Register,
        AccessPermissions.Roles.Remove,
        AccessPermissions.Permissions.Manage,
        AccessPermissions.Permissions.Register,
        AccessPermissions.Permissions.View,
        AccessPermissions.Sessions.Manage,
        AccessPermissions.Sessions.Register,
        AccessPermissions.Sessions.View
    ];

    private readonly IRoleQuery _roleQuery = Guard.AgainstNull(roleQuery);

    private readonly List<string> _systemPermissions =
    [
        AccessPermissions.Administrator,
        AccessPermissions.IdentityTenants.View,
        AccessPermissions.IdentityTenants.Remove,
        AccessPermissions.IdentityTenants.Manage,
        AccessPermissions.Tenants.View,
        AccessPermissions.Tenants.Register,
        AccessPermissions.Tenants.Manage
    ];

    private readonly ITenantQuery _tenantQuery = Guard.AgainstNull(tenantQuery);
    private readonly ITransactionScopeFactory _transactionScopeFactory = Guard.AgainstNull(transactionScopeFactory);

    public async Task ProcessMessageAsync(ConfigureApplication message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        foreach (var permission in _permissions)
        {
            using (var scope = _transactionScopeFactory.Create())
            {
                if (await _permissionQuery.ContainsAsync(new PermissionSpecification().AddName(permission), cancellationToken))
                {
                    continue;
                }

                _logger.LogDebug($"[permission/registration] : name = '{permission}'");

                var registerPermissionMessage = new RequestResponseMessage<RegisterPermission, PermissionRegistered>(
                    new()
                    {
                        Name = permission,
                        Status = (int)PermissionStatus.Active,
                        AuditIdentityName = "system",
                        AuditTenantId = _accessOptions.SystemTenantId
                    });

                await _mediator.SendAsync(registerPermissionMessage, cancellationToken);
                scope.Complete();
            }
        }

        foreach (var permission in _systemPermissions)
        {
            using (var scope = _transactionScopeFactory.Create())
            {
                if (await _permissionQuery.ContainsAsync(new PermissionSpecification().AddName(permission), cancellationToken))
                {
                    continue;
                }

                _logger.LogDebug($"[system-permission/registration] : name = '{permission}'");

                var registerPermissionMessage = new RequestResponseMessage<RegisterPermission, PermissionRegistered>(
                    new()
                    {
                        Name = permission,
                        Status = (int)PermissionStatus.Active,
                        TenantIds = [_accessOptions.SystemTenantId],
                        AuditIdentityName = "system",
                        AuditTenantId = _accessOptions.SystemTenantId
                    });

                await _mediator.SendAsync(registerPermissionMessage, cancellationToken);
                scope.Complete();
            }
        }

        var timeout = DateTimeOffset.Now.Add(message.Timeout);

        var permissionSpecification = new PermissionSpecification();

        foreach (var permission in _permissions)
        {
            permissionSpecification.AddName(permission);
        }

        while (await _permissionQuery.CountAsync(permissionSpecification, cancellationToken) != _permissions.Count && DateTimeOffset.Now < timeout)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).WaitAsync(cancellationToken);
        }

        if (await _permissionQuery.CountAsync(permissionSpecification, cancellationToken) != _permissions.Count)
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

        var tenantSpecification = new SqlServer.Models.Tenant.Specification()
            .AddId(_accessOptions.SystemTenantId);

        var systemTenantExists = await _tenantQuery.CountAsync(tenantSpecification, cancellationToken) > 0;

        _logger.LogInformation("[system-tenant] : id = '{SystemTenantId}' / exists = {SystemTenantExists}", _accessOptions.SystemTenantId, systemTenantExists);

        if (!systemTenantExists)
        {
            _logger.LogInformation("[RegisterTenant] : name = '{SystemTenantName}' / id = '{SystemTenantId}'", _accessOptions.SystemTenantName, _accessOptions.SystemTenantId);

            using (var scope = _transactionScopeFactory.Create())
            {
                var registerTenant = new RequestResponseMessage<RegisterTenant, TenantRegistered>(new()
                {
                    Id = _accessOptions.SystemTenantId,
                    Name = _accessOptions.SystemTenantName,
                    Status = 1,
                    AuditIdentityName = "system",
                    AuditTenantId = _accessOptions.SystemTenantId
                });

                await _mediator.SendAsync(registerTenant, cancellationToken);

                var tenantModel = await _accessDbContext.Tenants.FirstOrDefaultAsync(item => item.Id == _accessOptions.SystemTenantId, cancellationToken);

                if (tenantModel == null)
                {
                    _accessDbContext.Tenants.Add(new()
                    {
                        Id = _accessOptions.SystemTenantId,
                        Name = _accessOptions.SystemTenantName,
                        Status = 1
                    });

                    await _accessDbContext.SaveChangesAsync(cancellationToken);
                }

                scope.Complete();
            }

            if (await _tenantQuery.CountAsync(tenantSpecification, cancellationToken) == 0)
            {
                throw new ApplicationException(Resources.SystemTenantException);
            }
        }

        /*
         * Administrator Role
         *
         */

        var roleSpecification = new RoleSpecification()
            .AddName("Access Administrator")
            .IncludePermissions();

        var administratorExists = await _roleQuery.CountAsync(roleSpecification, cancellationToken) > 0;

        _logger.LogDebug($"[role] : name = 'Access Administrator' / exists = {administratorExists}");

        if (!administratorExists)
        {
            _logger.LogDebug("[role/registration] : name = 'Access Administrator'");

            using (var scope = _transactionScopeFactory.Create())
            {
                var registerRole = new RegisterRole("Access Administrator", new AuditInformation(_accessOptions.SystemTenantId, "system"));
                var registerRoleMessage = new RequestResponseMessage<RegisterRole, RoleRegistered>(registerRole);

                await _mediator.SendAsync(registerRoleMessage, cancellationToken);

                scope.Complete();
            }

            timeout = DateTimeOffset.Now.Add(message.Timeout);

            while (await _roleQuery.CountAsync(roleSpecification, cancellationToken) == 0 && DateTimeOffset.Now < timeout)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).WaitAsync(cancellationToken);
            }

            if (await _roleQuery.CountAsync(roleSpecification, cancellationToken) == 0)
            {
                throw new ApplicationException(Resources.AdministratorRoleException);
            }
        }

        var role = (await _roleQuery.SearchAsync(roleSpecification, cancellationToken)).SingleOrDefault();

        if (role == null)
        {
            throw new ApplicationException(Resources.AdministratorRoleException);
        }

        var administratorPermission = (await _permissionQuery.SearchAsync(new PermissionSpecification().AddName("access://*"), cancellationToken)).SingleOrDefault();

        if (administratorPermission == null)
        {
            throw new ApplicationException(Resources.AdministratorPermissionException);
        }

        await _mediator.SendAsync(new RequestResponseMessage<SetRolePermissionStatus, RolePermissionSet>(new()
        {
            Active = true,
            RoleId = role.Id,
            PermissionId = administratorPermission.Id,
            AuditIdentityName = "system",
            AuditTenantId = _accessOptions.SystemTenantId
        }), cancellationToken);

        timeout = DateTimeOffset.Now.Add(message.Timeout);

        var administratorPermissionsRegistered = false;

        while (!administratorPermissionsRegistered && DateTimeOffset.Now < timeout)
        {
            role = (await _roleQuery.SearchAsync(roleSpecification, cancellationToken)).SingleOrDefault();

            if (role == null)
            {
                throw new ApplicationException(Resources.AdministratorRoleException);
            }

            administratorPermissionsRegistered = role.RolePermissions.FirstOrDefault(item => item.Permission.Name.Equals(AccessPermissions.Administrator, StringComparison.InvariantCultureIgnoreCase)) != null;

            if (administratorPermissionsRegistered)
            {
                continue;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).WaitAsync(cancellationToken);
        }

        if (!administratorPermissionsRegistered)
        {
            throw new ApplicationException(Resources.AdministratorPermissionException);
        }

        if (await _identityQuery.CountAsync(new IdentitySpecification().WithRoleName("Access Administrator"), cancellationToken) == 0)
        {
            var generateHash = new GenerateHash { Value = message.AdministratorPassword };

            await _mediator.SendAsync(generateHash, cancellationToken);

            var registerIdentityMessage = new RequestResponseMessage<RegisterIdentity, IdentityRegistered>(new()
            {
                Name = message.AdministratorIdentityName,
                System = "system://access",
                PasswordHash = generateHash.Hash,
                AuditIdentityName = "system",
                AuditTenantId = _accessOptions.SystemTenantId,
                Activated = true
            });

            await _mediator.SendAsync(registerIdentityMessage, cancellationToken);
        }
    }
}