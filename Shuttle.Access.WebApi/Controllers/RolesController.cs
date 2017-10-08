using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;
using Shuttle.Esb;

namespace Shuttle.Access.WebApi
{
    public class RolesController : AccessApiController
    {
        private readonly IServiceBus _bus;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly ISystemRoleQuery _systemRoleQuery;

        public RolesController(IServiceBus bus, IDatabaseContextFactory databaseContextFactory,
            ISystemRoleQuery systemRoleQuery)
        {
            Guard.AgainstNull(databaseContextFactory, "databaseContextFactory");
            Guard.AgainstNull(bus, "bus");
            Guard.AgainstNull(systemRoleQuery, "systemRoleQuery");

            _databaseContextFactory = databaseContextFactory;
            _bus = bus;
            _systemRoleQuery = systemRoleQuery;
        }

        [RequiresPermission(SystemPermissions.Manage.Roles)]
        [Route("api/roles/setpermission")]
        public IHttpActionResult SetPermission([FromBody] SetRolePermissionModel model)
        {
            Guard.AgainstNull(model, "model");

            _bus.Send(new SetRolePermissionCommand
            {
                RoleId = model.RoleId,
                Permission = model.Permission,
                Active = model.Active
            });

            return Ok();
        }

        [RequiresPermission(SystemPermissions.Manage.Roles)]
        [Route("api/roles/permissionstatus")]
        public IHttpActionResult PermissionStatus([FromBody] RolePermissionStatusModel model)
        {
            Guard.AgainstNull(model, "model");

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

        public IHttpActionResult Get()
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

        [RequiresPermission(SystemPermissions.Manage.Roles)]
        public IHttpActionResult Get(Guid id)
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
        public IHttpActionResult Delete(Guid id)
        {
            _bus.Send(new RemoveRoleCommand
            {
                Id = id
            });

            return Ok();
        }

        [RequiresPermission(SystemPermissions.Manage.Roles)]
        public IHttpActionResult Post([FromBody] AddRoleModel model)
        {
            Guard.AgainstNull(model, "model");

            _bus.Send(new AddRoleCommand
            {
                Name = model.Name
            });

            return Ok();
        }

        [RequiresPermission(SystemPermissions.Manage.Roles)]
        [Route("api/roles/{id}/permissions")]
        public IHttpActionResult GetRoles(Guid id)
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(new
                {
                    Data = (from permission in _systemRoleQuery.Permissions(id)
                            select new
                            {
                                permission
                            }).ToList()
                });
            }
        }
    }
}