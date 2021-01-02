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

namespace Shuttle.Access.WebApi
{
    [Route("[controller]")]
    public class RolesController : Controller
    {
        private readonly IServiceBus _bus;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IRoleQuery _roleQuery;

        public RolesController(IServiceBus bus, IDatabaseContextFactory databaseContextFactory,
            IRoleQuery roleQuery)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(bus, nameof(bus));
            Guard.AgainstNull(roleQuery, nameof(roleQuery));

            _databaseContextFactory = databaseContextFactory;
            _bus = bus;
            _roleQuery = roleQuery;
        }

        [HttpPost("setpermission")]
        [RequiresPermission(Permissions.Register.Role)]
        public IActionResult SetPermission([FromBody] SetRolePermissionModel model)
        {
            try
            {
                model.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            _bus.Send(new SetRolePermissionCommand
            {
                RoleId = model.RoleId,
                Permission = model.Permission,
                Active = model.Active
            });

            return Ok();
        }

        [HttpPost("permissionstatus")]
        [RequiresPermission(Permissions.Register.Role)]
        public IActionResult PermissionStatus([FromBody] RolePermissionStatusModel model)
        {
            try
            {
                model.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            List<string> permissions;

            using (_databaseContextFactory.Create())
            {
                permissions = _roleQuery.Permissions(model.RoleId).ToList();
            }

            return Ok(
                from permission in model.Permissions
                select new
                {
                    Permission = permission,
                    Active = permissions.Find(item => item.Equals(permission)) != null
                }
            );
        }

        [HttpGet]
        [RequiresPermission(Permissions.View.Role)]
        public IActionResult Get()
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(_roleQuery.Search(new DataAccess.Query.Role.Specification()));
            }
        }

        [HttpGet("{id}")]
        [RequiresPermission(Permissions.View.Role)]
        public IActionResult Get(Guid id)
        {
            using (_databaseContextFactory.Create())
            {
                var role = _roleQuery.Search(new DataAccess.Query.Role.Specification().WithRoleId(id).IncludePermissions()).FirstOrDefault();

                return role != null
                    ? (IActionResult) Ok(role)
                    : BadRequest();
            }
        }

        [HttpDelete("{id}")]
        [RequiresPermission(Permissions.Remove.Role)]
        public IActionResult Delete(Guid id)
        {
            _bus.Send(new RemoveRoleCommand
            {
                Id = id
            });

            return Ok();
        }

        [HttpPost]
        [RequiresPermission(Permissions.Register.Role)]
        public IActionResult Post([FromBody] AddRoleModel model)
        {
            try
            {
                model.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            _bus.Send(new AddRoleCommand
            {
                Name = model.Name
            });

            return Ok();
        }
    }
}