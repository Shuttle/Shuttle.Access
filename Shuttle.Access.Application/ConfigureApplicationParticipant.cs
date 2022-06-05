using System;
using System.Collections.Generic;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Core.Threading;

namespace Shuttle.Access.Application
{
    public class ConfigureApplicationParticipant : IParticipant<ConfigureApplication>
    {
        private readonly IMediator _mediator;
        private readonly IRoleQuery _roleQuery;
        private readonly IPermissionQuery _permissionQuery;
        private readonly IIdentityQuery _identityQuery;

        private readonly List<string> _permissions = new List<string>
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

        public void ProcessMessage(IParticipantContext<ConfigureApplication> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var roleSpecification = new DataAccess.Query.Role.Specification().AddName("Administrator");

            var administratorExists = _roleQuery.Count(roleSpecification) > 0;

            if (!administratorExists)
            {
                var registerRoleMessage = new RequestResponseMessage<RegisterRole, RoleRegistered>(new RegisterRole
                {
                    Name = "Administrator"
                });

                _mediator.Send(registerRoleMessage);
            }

            foreach (var permission in _permissions)
            {
                if (_permissionQuery.Contains(new DataAccess.Query.Permission.Specification().AddName(permission)))
                {
                    continue;
                }

                var registerPermissionMessage = new RequestResponseMessage<RegisterPermission, PermissionRegistered>(
                    new RegisterPermission
                    {
                        Name = permission,
                        Status = (int)PermissionStatus.Active
                    });

                _mediator.Send(registerPermissionMessage);
            }

            if (_identityQuery.Count(new DataAccess.Query.Identity.Specification()) == 0)
            {
                if (!administratorExists)
                {
                    // wait for role projection
                    var timeout = DateTime.Now.AddSeconds(15);

                    while (_roleQuery.Count(roleSpecification) == 0 && DateTime.Now < timeout)
                    {
                        ThreadSleep.While(500, context.CancellationToken);
                    }

                    if (_roleQuery.Count(roleSpecification) == 0)
                    {
                        throw new ApplicationException(Resources.AdministratorRoleException);
                    }
                }

                var generateHash = new GenerateHash { Value = "admin" };

                _mediator.Send(generateHash);
;
                var registerIdentityMessage = new RequestResponseMessage<RegisterIdentity, IdentityRegistered>(new RegisterIdentity
                {
                    Name = "admin",
                    System = "system://access",
                    PasswordHash = generateHash.Hash,
                    RegisteredBy = "system",
                    Activated = true
                });

                _mediator.Send(registerIdentityMessage);
            }
        }
    }
}