using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.WebApi.Controllers.v1
{
    [Route("[controller]", Order = 1)]
    [Route("v{version:apiVersion}/[controller]", Order = 2)]
    [ApiVersion("1")]
    public class StatisticsController : Controller
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IRoleQuery _roleQuery;
        private readonly IIdentityQuery _identityQuery;
        private readonly IPermissionQuery _permissionQuery;

        public StatisticsController(IDatabaseContextFactory databaseContextFactory, IRoleQuery roleQuery, IIdentityQuery identityQuery, IPermissionQuery permissionQuery)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(roleQuery, nameof(roleQuery));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));
            Guard.AgainstNull(permissionQuery, nameof(permissionQuery));

            _databaseContextFactory = databaseContextFactory;
            _roleQuery = roleQuery;
            _identityQuery = identityQuery;
            _permissionQuery = permissionQuery;
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(new
                {
                    IdentityCount = _identityQuery.Count(new DataAccess.Query.Identity.Specification()),
                    RoleCount = _roleQuery.Count(new DataAccess.Query.Role.Specification()),
                    PermissionCount = _permissionQuery.Count(new DataAccess.Query.Permission.Specification())
                });
            }
        }
    }
}
