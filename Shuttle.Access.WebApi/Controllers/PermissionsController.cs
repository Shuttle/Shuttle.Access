using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.WebApi
{
    [Route("api/[controller]")]
    public class PermissionsController : Controller
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IPermissionQuery _permissionQuery;

        public PermissionsController(IDatabaseContextFactory databaseContextFactory, IPermissionQuery permissionQuery)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(permissionQuery, nameof(permissionQuery));

            _databaseContextFactory = databaseContextFactory;
            _permissionQuery = permissionQuery;
        }

        [HttpGet]
        public IActionResult Get()
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(new
                {
                    Data = _permissionQuery.Available()
                        .Select(permission => new
                        {
                            Permission = permission
                        }).ToList()
                });
            }
        }
    }
}