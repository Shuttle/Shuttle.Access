using System;
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

        [HttpPatch("{id}/permissions")]
        [RequiresPermission(Permissions.Register.Role)]
        public IActionResult SetPermission(Guid id, [FromBody] SetRolePermissionStatus message)
        {
            if (message != null)
            {
                message.RoleId = id;
            }

            try
            {
                message.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            _bus.Send(message);

            return Accepted();
        }

        [HttpGet("{id}/permissions")]
        [RequiresPermission(Permissions.Register.Role)]
        public IActionResult GetPermissions(Guid id, DateTime startDateRegistered)
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(_roleQuery.Permissions(new DataAccess.Query.Role.Specification()
                    .WithRoleId(id)
                    .WithStartDateRegistered(startDateRegistered)).Select(item => item.Permission).ToList());
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

        [HttpGet("{id}")]
        [RequiresPermission(Permissions.View.Role)]
        public IActionResult Get(Guid id)
        {
            using (_databaseContextFactory.Create())
            {
                var role = _roleQuery
                    .Search(new DataAccess.Query.Role.Specification().WithRoleId(id).IncludePermissions())
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
            _bus.Send(new RemoveRole
            {
                Id = id
            });

            return Accepted();
        }

        [HttpPost]
        [RequiresPermission(Permissions.Register.Role)]
        public IActionResult Post([FromBody] AddRole message)
        {
            try
            {
                message.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            _bus.Send(message);

            return Accepted();
        }
    }
}