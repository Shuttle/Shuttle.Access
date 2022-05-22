using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Mvc;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;

namespace Shuttle.Access.WebApi.v1
{
    [Route("[controller]", Order = 1)]
    [Route("v{version:apiVersion}/[controller]", Order = 2)]
    [ApiVersion("1")]
    public class PermissionsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDatabaseContextFactory _databaseContextFactory;

        private readonly List<string> _emptyAnonymousPermissions = new();
        private readonly IPermissionQuery _permissionQuery;
        private readonly IServiceBus _serviceBus;

        public PermissionsController(IServiceBus serviceBus, IAuthorizationService authorizationService,
            IDatabaseContextFactory databaseContextFactory, IPermissionQuery permissionQuery)
        {
            Guard.AgainstNull(serviceBus, nameof(serviceBus));
            Guard.AgainstNull(authorizationService, nameof(authorizationService));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(permissionQuery, nameof(permissionQuery));

            _serviceBus = serviceBus;
            _authorizationService = authorizationService;
            _databaseContextFactory = databaseContextFactory;
            _permissionQuery = permissionQuery;
        }

        [HttpGet("anonymous")]
        public IActionResult AnonymousPermissions()
        {
            List<string> permissions;

            using (_databaseContextFactory.Create())
            {
                permissions = _authorizationService is IAnonymousPermissions anonymousPermissions
                    ? new List<string>(anonymousPermissions.AnonymousPermissions())
                    : _emptyAnonymousPermissions;
            }

            return Ok(new AnonymousPermissions
            {
                IsIdentityRequired = permissions.Contains(Permissions.Register.IdentityRequired),
                Permissions = (
                    from permission in permissions
                    select permission
                ).ToList()
            });
        }

        [HttpGet]
        [RequiresSession]
        public IActionResult Get()
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(_permissionQuery.Search(new DataAccess.Query.Permission.Specification()).ToList());
            }
        }

        [HttpPost]
        [RequiresPermission(Permissions.Register.Permission)]
        public IActionResult Post([FromBody] RegisterPermission message)
        {
            try
            {
                message.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            _serviceBus.Send(message);

            return Accepted();
        }

        [HttpDelete("{id}")]
        [RequiresPermission(Permissions.Remove.Permission)]
        public IActionResult Delete(Guid id)
        {
            _serviceBus.Send(new RemovePermission
            {
                Id = id
            });

            return Accepted();
        }
    }
}