using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Mvc;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.WebApi
{
    [Route("[controller]")]
    public class PermissionsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDatabaseContextFactory _databaseContextFactory;

        private readonly List<string> _emptyAnonymousPermissions = new List<string>();
        private readonly IPermissionQuery _permissionQuery;

        public PermissionsController(IAuthorizationService authorizationService,
            IDatabaseContextFactory databaseContextFactory, IPermissionQuery permissionQuery)
        {
            Guard.AgainstNull(authorizationService, nameof(authorizationService));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(permissionQuery, nameof(permissionQuery));

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

            return Ok(new
            {
                IsUserRequired = permissions.Contains(SystemPermissions.Register.UserRequired),
                Permissions =
                    from permission in permissions
                    select new
                    {
                        Permission = permission
                    }
            });
        }

        [HttpGet]
        [RequiresSession]
        public IActionResult Get()
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(_permissionQuery.Available()
                    .Select(permission => new
                    {
                        Permission = permission
                    }).ToList()
                );
            }
        }

        [HttpPost]
        [RequiresPermission(SystemPermissions.Register.Permissions)]
        public IActionResult Post([FromBody] AvailablePermissionModel model)
        {
            try
            {
                model.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            using (_databaseContextFactory.Create())
            {
                _permissionQuery.Register(model.Permission);
            }

            return Ok();
        }

        [HttpDelete]
        public IActionResult Delete([FromBody] AvailablePermissionModel model)
        {
            try
            {
                model.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            using (_databaseContextFactory.Create())
            {
                _permissionQuery.Remove(model.Permission);
            }

            return Ok();
        }

    }
}