using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Mvc;
using Shuttle.Access.WebApi.v1.Specifications;
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
        private readonly IDatabaseContextFactory _databaseContextFactory;

        private readonly IPermissionQuery _permissionQuery;
        private readonly IServiceBus _serviceBus;

        public PermissionsController(IServiceBus serviceBus, IDatabaseContextFactory databaseContextFactory, IPermissionQuery permissionQuery)
        {
            Guard.AgainstNull(serviceBus, nameof(serviceBus));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(permissionQuery, nameof(permissionQuery));

            _serviceBus = serviceBus;
            _databaseContextFactory = databaseContextFactory;
            _permissionQuery = permissionQuery;
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

        [HttpGet("{id}")]
        [RequiresSession]
        public IActionResult Get(Guid id)
        {
            using (_databaseContextFactory.Create())
            {
                var permission = _permissionQuery.Search(new DataAccess.Query.Permission.Specification().AddId(id))
                    .SingleOrDefault();

                return permission != null ? Ok(permission) : BadRequest();
            }
        }

        [HttpPost("search")]
        [RequiresSession]
        public IActionResult Search([FromBody] PermissionSpecification specification)
        {
            if (specification == null)
            {
                return BadRequest();
            }

            using (_databaseContextFactory.Create())
            {
                return Ok(_permissionQuery.Search(specification.Create()).ToList());
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

        [HttpPatch("{id}/name")]
        [RequiresPermission(Permissions.Register.Permission)]
        public IActionResult SetName(Guid id, [FromBody] SetPermissionName message)
        {
            try
            {
                message.Id = id;
                message.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            _serviceBus.Send(message);

            return Accepted();
        }

        [HttpPatch("{id}")]
        [RequiresPermission(Permissions.Status.Permission)]
        public IActionResult SetStatus(Guid id, [FromBody] SetPermissionStatus message)
        {
            try
            {
                message.Id = id;
                message.ApplyInvariants();
                Guard.AgainstUndefinedEnum<PermissionStatus>(message.Status, nameof(message.Status));
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