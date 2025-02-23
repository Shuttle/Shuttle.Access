using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

    public ConfigureApplicationParticipant(ILogger<ConfigureApplicationParticipant> logger, IMediator mediator, IRoleQuery roleQuery, IPermissionQuery permissionQuery, IIdentityQuery identityQuery)
    {
        _logger = Guard.AgainstNull(logger);
        _mediator = Guard.AgainstNull(mediator);
        _roleQuery = Guard.AgainstNull(roleQuery);
        _permissionQuery = Guard.AgainstNull(permissionQuery);
        _identityQuery = Guard.AgainstNull(identityQuery);
    }

    public async Task ProcessMessageAsync(IParticipantContext<ConfigureApplication> context)
    {
        Guard.AgainstNull(context);

        var roleSpecification = new DataAccess.Role.Specification().AddName("Administrator");

        var administratorExists = await _roleQuery.CountAsync(roleSpecification) > 0;

        _logger.LogDebug($"[role] : name = 'Administrator' / exists = {administratorExists}");

        if (!administratorExists)
        {
            _logger.LogDebug("[role/registration] : name = 'Administrator'");

            var registerRoleMessage = new RequestResponseMessage<RegisterRole, RoleRegistered>(new()
            {
                Name = "Administrator"
            });

            await _mediator.SendAsync(registerRoleMessage);

            var timeout = DateTimeOffset.Now.AddSeconds(15);

            while (await _roleQuery.CountAsync(roleSpecification) == 0 && DateTimeOffset.Now < timeout)
            {
                Task.Delay(TimeSpan.FromMilliseconds(500), context.CancellationToken).Wait();
            }

            if (await _roleQuery.CountAsync(roleSpecification) == 0)
            {
                throw new ApplicationException(Resources.AdministratorRoleException);
            }
        }

        foreach (var permission in _permissions)
        {
            if (await _permissionQuery.ContainsAsync(new DataAccess.Permission.Specification().AddName(permission)))
            {
                continue;
            }

            var registerPermissionMessage = new RequestResponseMessage<RegisterPermission, PermissionRegistered>(
                new()
                {
                    Name = permission,
                    Status = (int)PermissionStatus.Active
                });

            await _mediator.SendAsync(registerPermissionMessage);
        }

        if (await _identityQuery.CountAsync(new()) == 0)
        {
            var generateHash = new GenerateHash { Value = "admin" };

            await _mediator.SendAsync(generateHash);

            var registerIdentityMessage = new RequestResponseMessage<RegisterIdentity, IdentityRegistered>(new()
            {
                Name = "admin",
                System = "system://access",
                GeneratedPassword = "admin",
                PasswordHash = generateHash.Hash,
                RegisteredBy = "system",
                Activated = true
            });

            await _mediator.SendAsync(registerIdentityMessage);
        }
    }
}
