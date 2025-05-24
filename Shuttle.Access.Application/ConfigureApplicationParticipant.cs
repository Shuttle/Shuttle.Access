using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class ConfigureApplicationParticipant : IParticipant<ConfigureApplication>
{
    private readonly IIdentityQuery _identityQuery;
    private readonly ILogger<ConfigureApplicationParticipant> _logger;
    private readonly IMediator _mediator;
    private readonly IPermissionQuery _permissionQuery;
    private readonly IRoleQuery _roleQuery;

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

    private readonly AccessOptions _accessOptions;

    public ConfigureApplicationParticipant(IOptions<AccessOptions> accessOptions, ILogger<ConfigureApplicationParticipant> logger, IMediator mediator, IRoleQuery roleQuery, IPermissionQuery permissionQuery, IIdentityQuery identityQuery)
    {
        _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
        _logger = Guard.AgainstNull(logger);
        _mediator = Guard.AgainstNull(mediator);
        _roleQuery = Guard.AgainstNull(roleQuery);
        _permissionQuery = Guard.AgainstNull(permissionQuery);
        _identityQuery = Guard.AgainstNull(identityQuery);
    }

    public async Task ProcessMessageAsync(IParticipantContext<ConfigureApplication> context)
    {
        Guard.AgainstNull(context);

        foreach (var permission in _permissions)
        {
            if (await _permissionQuery.ContainsAsync(new DataAccess.Permission.Specification().AddName(permission)))
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

            await _mediator.SendAsync(registerPermissionMessage);
        }

        var timeout = DateTimeOffset.Now.Add(_accessOptions.Configuration.Timeout);

        var permissionSpecification = new DataAccess.Permission.Specification();

        foreach (var permission in _permissions)
        {
            permissionSpecification.AddName(permission);
        }

        while (await _permissionQuery.CountAsync(permissionSpecification) != _permissions.Count && DateTimeOffset.Now < timeout)
        {
            Task.Delay(TimeSpan.FromMilliseconds(500), context.CancellationToken).Wait();
        }

        if (await _permissionQuery.CountAsync(permissionSpecification) != _permissions.Count)
        {
            throw new ApplicationException(Resources.PermissionsException);
        }

        if (!_accessOptions.Configuration.ShouldConfigure)
        {
            return;
        }

        var roleSpecification = new DataAccess.Role.Specification()
            .AddName("Access Administrator")
            .IncludePermissions();

        var administratorExists = await _roleQuery.CountAsync(roleSpecification) > 0;

        _logger.LogDebug($"[role] : name = 'Access Administrator' / exists = {administratorExists}");

        if (!administratorExists)
        {
            _logger.LogDebug("[role/registration] : name = 'Access Administrator'");

            var registerRole = new RegisterRole("Access Administrator");

            var registerRoleMessage = new RequestResponseMessage<RegisterRole, RoleRegistered>(registerRole);

            await _mediator.SendAsync(registerRoleMessage);

            timeout = DateTimeOffset.Now.Add(_accessOptions.Configuration.Timeout);

            while (await _roleQuery.CountAsync(roleSpecification) == 0 && DateTimeOffset.Now < timeout)
            {
                Task.Delay(TimeSpan.FromMilliseconds(500), context.CancellationToken).Wait();
            }

            if (await _roleQuery.CountAsync(roleSpecification) == 0)
            {
                throw new ApplicationException(Resources.AdministratorRoleException);
            }
        }

        var role  = (await _roleQuery.SearchAsync(roleSpecification)).SingleOrDefault();

        if (role == null)
        {
            throw new ApplicationException(Resources.AdministratorRoleException);
        }

        var administratorPermission = (await _permissionQuery.SearchAsync(new DataAccess.Permission.Specification().AddName("access://*"))).SingleOrDefault();

        if (administratorPermission == null)
        {
            throw new ApplicationException(Resources.AdministratorPermissionException);
        }

        await _mediator.SendAsync(new RequestResponseMessage<SetRolePermission, RolePermissionSet>(new()
        {
            Active = true,
            RoleId = role.Id,
            PermissionId = administratorPermission.Id
        }));

        timeout = DateTimeOffset.Now.Add(_accessOptions.Configuration.Timeout);

        var administratorPermissionsRegistered = false;

        while (!administratorPermissionsRegistered && DateTimeOffset.Now < timeout)
        {
            role = (await _roleQuery.SearchAsync(roleSpecification)).SingleOrDefault();

            if (role == null)
            {
                throw new ApplicationException(Resources.AdministratorRoleException);
            }

            administratorPermissionsRegistered = role.Permissions.FirstOrDefault(item => item.Name.Equals(AccessPermissions.Administrator, StringComparison.InvariantCultureIgnoreCase)) != null;

            if (administratorPermissionsRegistered)
            {
                continue;
            }

            Task.Delay(TimeSpan.FromMilliseconds(500), context.CancellationToken).Wait();
        }

        if (!administratorPermissionsRegistered)
        {
            throw new ApplicationException(Resources.AdministratorPermissionException);
        }

        if (await _identityQuery.CountAsync(new DataAccess.Identity.Specification().WithRoleName("Access Administrator")) == 0)
        {
            var generateHash = new GenerateHash { Value = _accessOptions.Configuration.AdministratorPassword };

            await _mediator.SendAsync(generateHash);

            var registerIdentityMessage = new RequestResponseMessage<RegisterIdentity, IdentityRegistered>(new()
            {
                Name = _accessOptions.Configuration.AdministratorIdentityName,
                System = "system://access",
                PasswordHash = generateHash.Hash,
                RegisteredBy = "system",
                Activated = true
            });

            await _mediator.SendAsync(registerIdentityMessage);
        }
    }
}
