using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages;
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
    public class RolesController : Controller
    {
        private readonly IServiceBus _serviceBus;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IRoleQuery _roleQuery;

        public RolesController(IServiceBus serviceBus, IDatabaseContextFactory databaseContextFactory,
            IRoleQuery roleQuery)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(serviceBus, nameof(serviceBus));
            Guard.AgainstNull(roleQuery, nameof(roleQuery));

            _databaseContextFactory = databaseContextFactory;
            _serviceBus = serviceBus;
            _roleQuery = roleQuery;
        }

        [HttpPatch("{id}/name")]
        [RequiresPermission(Permissions.Register.Role)]
        public IActionResult SetName(Guid id, [FromBody] SetRoleName message)
        {
            try
            {
                message.ApplyInvariants();
                message.Id = id;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            _serviceBus.Send(message);

            return Accepted();
        }

        [HttpPatch("{id}/permissions")]
        [RequiresPermission(Permissions.Register.Role)]
        public IActionResult SetPermissionStatus(Guid id, [FromBody] SetRolePermission message)
        {
            try
            {
                message.ApplyInvariants();
                message.RoleId = id;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            _serviceBus.Send(message);

            return Accepted();
        }

        [HttpPost("{id}/permissions/availability")]
        [RequiresPermission(Permissions.Register.Role)]
        public IActionResult PermissionsSearch(Guid id, [FromBody] Identifiers<Guid> identifiers)
        {
            try
            {
                identifiers.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            using (_databaseContextFactory.Create())
            {
                var permissions = _roleQuery.Permissions(new DataAccess.Query.Role.Specification().AddId(id)).ToList();

                return Ok(from permissionId in identifiers.Values
                    select new IdentifierAvailability<Guid>()
                    {
                        Id = permissionId,
                        Active = permissions.Any(item => item.Id.Equals(permissionId))
                    });
            }
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

        [HttpGet("{value}")]
        [RequiresPermission(Permissions.View.Role)]
        public IActionResult Get(string value)
        {
            using (_databaseContextFactory.Create())
            {
                var specification = new DataAccess.Query.Role.Specification();

                if (Guid.TryParse(value, out var id))
                {
                    specification.AddId(id);
                }
                else
                {
                    specification.AddName(value);
                }
                
                var role = _roleQuery
                    .Search(specification.IncludePermissions())
                    .FirstOrDefault();

                return role != null
                    ? Ok(role)
                    : BadRequest();
            }
        }

        [HttpDelete("{id}")]
        [RequiresPermission(Permissions.Remove.Role)]
        public IActionResult Delete(Guid id)
        {
            _serviceBus.Send(new RemoveRole
            {
                Id = id
            });

            return Accepted();
        }

        [HttpPost]
        [RequiresPermission(Permissions.Register.Role)]
        public IActionResult Post([FromBody] RegisterRole message)
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
    }
}