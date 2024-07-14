using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Core.Threading;
using PermissionSpecification = Shuttle.Access.DataAccess.PermissionSpecification;

namespace Shuttle.Access.Application
{
    public class ConfigureApplicationParticipant : IAsyncParticipant<ConfigureApplication>
    {
        private readonly IMediator _mediator;
        private readonly IRoleQuery _roleQuery;
        private readonly IPermissionQuery _permissionQuery;
        private readonly IIdentityQuery _identityQuery;

        private readonly List<string> _permissions = new()
        {
            "access://identity/view",
            "access://identity/register",
            "access://identity/register-session",
            "access://identity/remove",
            "access://identity/view",
            "access://permission/view",
            "access://permission/register",
            "access://permission/status",
            "access://dashboard/view",
            "access://role/view",
            "access://role/register",
            "access://role/remove",
            "access://sessions/view"
        };

        public ConfigureApplicationParticipant(IMediator mediator, IRoleQuery roleQuery, IPermissionQuery permissionQuery, IIdentityQuery identityQuery)
        {
            Guard.AgainstNull(mediator, nameof(mediator));
            Guard.AgainstNull(roleQuery, nameof(roleQuery));
            Guard.AgainstNull(permissionQuery, nameof(permissionQuery));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));
        
            _mediator = mediator;
            _roleQuery = roleQuery;
            _permissionQuery = permissionQuery;
            _identityQuery = identityQuery;
        }

        public async Task ProcessMessageAsync(IParticipantContext<ConfigureApplication> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var roleSpecification = new RoleSpecification().AddName("Administrator");

            var administratorExists = await _roleQuery.CountAsync(roleSpecification) > 0;

            if (!administratorExists)
            {
                var registerRoleMessage = new RequestResponseMessage<RegisterRole, RoleRegistered>(new RegisterRole
                {
                    Name = "Administrator"
                });

                await _mediator.SendAsync(registerRoleMessage);

                var timeout = DateTime.Now.AddSeconds(15);

                while (await _roleQuery.CountAsync(roleSpecification) == 0 && DateTime.Now < timeout)
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
                if (await _permissionQuery.ContainsAsync(new PermissionSpecification().AddName(permission)))
                {
                    continue;
                }

                var registerPermissionMessage = new RequestResponseMessage<RegisterPermission, PermissionRegistered>(
                    new RegisterPermission
                    {
                        Name = permission,
                        Status = (int)PermissionStatus.Active
                    });

                await _mediator.SendAsync(registerPermissionMessage);
            }

            if (await _identityQuery.CountAsync(new IdentitySpecification()) == 0)
            {
                var generateHash = new GenerateHash { Value = "admin" };

                await _mediator.SendAsync(generateHash);

                var registerIdentityMessage = new RequestResponseMessage<RegisterIdentity, IdentityRegistered>(new RegisterIdentity
                {
                    Name = "admin",
                    System = "system://access",
                    PasswordHash = generateHash.Hash,
                    RegisteredBy = "system",
                    Activated = true
                });

                await _mediator.SendAsync(registerIdentityMessage);
            }
        }
    }
}