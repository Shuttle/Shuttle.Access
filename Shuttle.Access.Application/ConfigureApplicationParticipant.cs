using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Access.Application;

public class ConfigureApplicationParticipant(IOptions<AccessOptions> accessOptions, ILogger<ConfigureApplicationParticipant> logger, ITransactionScopeFactory transactionScopeFactory, IMediator mediator, ITenantQuery tenantQuery, IRoleQuery roleQuery, IPermissionQuery permissionQuery, IIdentityQuery identityQuery, AccessDbContext accessDbContext)
    : IParticipant<ConfigureApplication>
{
    private readonly ILogger<ConfigureApplicationParticipant> _logger = Guard.AgainstNull(logger);
    private readonly AccessOptions _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
    private readonly ITransactionScopeFactory _transactionScopeFactory = Guard.AgainstNull(transactionScopeFactory);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);
    private readonly IPermissionQuery _permissionQuery = Guard.AgainstNull(permissionQuery);
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

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

    public async Task ProcessMessageAsync(ConfigureApplication message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        using (var scope = _transactionScopeFactory.Create())
        {
            foreach (var permission in _permissions)
            {
                if (await _permissionQuery.ContainsAsync(new SqlServer.Models.Permission.Specification().AddName(permission), cancellationToken))
                {
                    continue;
                }

                _logger.LogDebug($"[permission/registration] : name = '{permission}'");

                var registerPermissionMessage = new RequestResponseMessage<RegisterPermission, PermissionRegistered>(
                    new()
                    {
                        Name = permission,
                        Status = (int)PermissionStatus.Active,
                        AuditIdentityName = "system"
                    });

                await _mediator.SendAsync(registerPermissionMessage, cancellationToken);
            }

            foreach (var permission in _systemPermissions)
            {
                if (await _permissionQuery.ContainsAsync(new SqlServer.Models.Permission.Specification().AddName(permission), cancellationToken))
                {
                    continue;
                }

                _logger.LogDebug($"[system-permission/registration] : name = '{permission}'");

                var registerPermissionMessage = new RequestResponseMessage<RegisterPermission, PermissionRegistered>(
                    new()
                    {
                        Name = permission,
                        Status = (int)PermissionStatus.Active,
                        AuditIdentityName = "system",
                        TenantIds = [_accessOptions.SystemTenantId]
                    });

                await _mediator.SendAsync(registerPermissionMessage, cancellationToken);
            }

            scope.Complete();
        }

        var timeout = DateTimeOffset.Now.Add(_accessOptions.Configuration.Timeout);

        var permissionSpecification = new SqlServer.Models.Permission.Specification();

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

        if (!_accessOptions.Configuration.ShouldConfigure)
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
                    AuditIdentityName = "system"
                });

                await _mediator.SendAsync(registerTenant, cancellationToken);

                var tenantModel = await _accessDbContext.Tenants.FirstOrDefaultAsync(item => item.Id == _accessOptions.SystemTenantId, cancellationToken: cancellationToken);

                if (tenantModel == null)
                {
                    _accessDbContext.Tenants.Add(new()
                    {
                        Id = _accessOptions.SystemTenantId,
                        Name = _accessOptions.SystemTenantName
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

        var roleSpecification = new SqlServer.Models.Role.Specification()
            .AddName("Access Administrator")
            .IncludePermissions();

        var administratorExists = await _roleQuery.CountAsync(roleSpecification, cancellationToken) > 0;

        _logger.LogDebug($"[role] : name = 'Access Administrator' / exists = {administratorExists}");

        if (!administratorExists)
        {
            _logger.LogDebug("[role/registration] : name = 'Access Administrator'");

            using (var scope = _transactionScopeFactory.Create())
            {
                var registerRole = new RegisterRole(_accessOptions.SystemTenantId, "Access Administrator", "system");
                var registerRoleMessage = new RequestResponseMessage<RegisterRole, RoleRegistered>(registerRole);

                await _mediator.SendAsync(registerRoleMessage, cancellationToken);

                scope.Complete();
            }

            timeout = DateTimeOffset.Now.Add(_accessOptions.Configuration.Timeout);

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

        var administratorPermission = (await _permissionQuery.SearchAsync(new SqlServer.Models.Permission.Specification().AddName("access://*"), cancellationToken)).SingleOrDefault();

        if (administratorPermission == null)
        {
            throw new ApplicationException(Resources.AdministratorPermissionException);
        }

        await _mediator.SendAsync(new RequestResponseMessage<SetRolePermissionStatus, RolePermissionSet>(new()
        {
            Active = true,
            RoleId = role.Id,
            PermissionId = administratorPermission.Id,
            AuditIdentityName = "system"
        }), cancellationToken);

        timeout = DateTimeOffset.Now.Add(_accessOptions.Configuration.Timeout);

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

        if (await _identityQuery.CountAsync(new SqlServer.Models.Identity.Specification().WithRoleName("Access Administrator"), cancellationToken) == 0)
        {
            var generateHash = new GenerateHash { Value = _accessOptions.Configuration.AdministratorPassword };

            await _mediator.SendAsync(generateHash, cancellationToken);

            var registerIdentityMessage = new RequestResponseMessage<RegisterIdentity, IdentityRegistered>(new()
            {
                Name = _accessOptions.Configuration.AdministratorIdentityName,
                System = "system://access",
                PasswordHash = generateHash.Hash,
                AuditIdentityName = "system",
                Activated = true
            });

            await _mediator.SendAsync(registerIdentityMessage, cancellationToken);
        }
    }
}