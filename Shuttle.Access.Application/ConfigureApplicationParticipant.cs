using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class ConfigureApplicationParticipant(IOptions<AccessOptions> accessOptions, ILogger<ConfigureApplicationParticipant> logger, IMediator mediator, IRoleQuery roleQuery, IPermissionQuery permissionQuery, IIdentityQuery identityQuery)
    : IParticipant<ConfigureApplication>
{
    private readonly AccessOptions _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly ILogger<ConfigureApplicationParticipant> _logger = Guard.AgainstNull(logger);
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);
    private readonly IPermissionQuery _permissionQuery = Guard.AgainstNull(permissionQuery);

    private readonly List<string> _permissions =
    [
        AccessPermissions.Administrator,
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

    public async Task ProcessMessageAsync(ConfigureApplication message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

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
                    Status = (int)PermissionStatus.Active
                });

            await _mediator.SendAsync(registerPermissionMessage, cancellationToken);
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

        var roleSpecification = new SqlServer.Models.Role.Specification()
            .AddName("Access Administrator")
            .IncludePermissions();

        var administratorExists = await _roleQuery.CountAsync(roleSpecification, cancellationToken) > 0;

        _logger.LogDebug($"[role] : name = 'Access Administrator' / exists = {administratorExists}");

        if (!administratorExists)
        {
            _logger.LogDebug("[role/registration] : name = 'Access Administrator'");

            var registerRole = new RegisterRole("Access Administrator");

            var registerRoleMessage = new RequestResponseMessage<RegisterRole, RoleRegistered>(registerRole);

            await _mediator.SendAsync(registerRoleMessage, cancellationToken);

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

        await _mediator.SendAsync(new RequestResponseMessage<SetRolePermission, RolePermissionSet>(new()
        {
            Active = true,
            RoleId = role.Id,
            PermissionId = administratorPermission.Id
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

            administratorPermissionsRegistered = role.RolePermissions.FirstOrDefault(item => item.Role.Name.Equals(AccessPermissions.Administrator, StringComparison.InvariantCultureIgnoreCase)) != null;

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
                RegisteredBy = "system",
                Activated = true
            });

            await _mediator.SendAsync(registerIdentityMessage, cancellationToken);
        }
    }
}