using System.Collections.Generic;
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

        [HttpGet("/anonymous")]
        public IActionResult AnonymousPermissions()
        {
            var permissions = _authorizationService is IAnonymousPermissions anonymousPermissions
                ? new List<string>(anonymousPermissions.AnonymousPermissions())
                : _emptyAnonymousPermissions;

            return Ok(new
            {
                Data = new
                {
                    IsUserRequired = permissions.Contains(SystemPermissions.Register.UserRequired),
                    Permissions =
                        from permission in permissions
                        select new
                        {
                            Permission = permission
                        }
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