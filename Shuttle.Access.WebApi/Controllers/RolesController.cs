using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;

namespace Shuttle.Access.WebApi
{
    public class RolesController : AccessController
    {
        private readonly IServiceBus _bus;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly ISystemRoleQuery _systemRoleQuery;

        public RolesController(IServiceBus bus, IDatabaseContextFactory databaseContextFactory,
            ISystemRoleQuery systemRoleQuery)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(bus, nameof(bus));
            Guard.AgainstNull(systemRoleQuery, nameof(systemRoleQuery));

            _databaseContextFactory = databaseContextFactory;
            _bus = bus;
            _systemRoleQuery = systemRoleQuery;
        }

        [RequiresPermission(SystemPermissions.Manage.Roles)]
        [HttpPost("setpermission")]
        public IActionResult SetPermission([FromBody] SetRolePermissionModel model)
        {
            Guard.AgainstNull(model, nameof(model));

            _bus.Send(new SetRolePermissionCommand
            {
                RoleId = model.RoleId,
                Permission = model.Permission,
                Active = model.Active
            });

            return Ok();
        }

        [RequiresPermission(SystemPermissions.Manage.Roles)]
        [HttpPost("permissionstatus")]
        public IActionResult PermissionStatus([FromBody] RolePermissionStatusModel model)
        {
            Guard.AgainstNull(model, nameof(model));

            List<string> permissions;

            using (_databaseContextFactory.Create())
            {
                permissions = _systemRoleQuery.Permissions(model.RoleId).ToList();
            }

            return Ok(
                new
                {
                    Data = from permission in model.Permissions
                    select new
                    {
                        Permission = permission,
                        Active = permissions.Find(item => item.Equals(permission)) != null
                    }
                });
        }

        [HttpGet]
        public IActionResult Get()
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(new
                {
                    Data = from row in _systemRoleQuery.Search()
                    select new
                    {
                        Id = SystemRoleColumns.Id.MapFrom(row),
                        RoleName = SystemRoleColumns.RoleName.MapFrom(row)
                    }
                });
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(new
                {
                    Data = _systemRoleQuery.Get(id)
                });
            }
        }

        [RequiresPermission(SystemPermissions.Manage.Roles)]
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _bus.Send(new RemoveRoleCommand
            {
                Id = id
            });

            return Ok();
        }

        [RequiresPermission(SystemPermissions.Manage.Roles)]
        [HttpPost]
        public IActionResult Post([FromBody] AddRoleModel model)
        {
            Guard.AgainstNull(model, nameof(model));

            _bus.Send(new AddRoleCommand
            {
                Name = model.Name
            });

            return Ok();
        }
    }
}