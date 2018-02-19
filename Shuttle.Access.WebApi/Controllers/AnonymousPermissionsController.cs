using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi
{
    public class AnonymousPermissionsController : AccessController
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly List<string> _emptyAnonymousPermissions = new List<string>();

        public AnonymousPermissionsController(IAuthorizationService authorizationService)
        {
            Guard.AgainstNull(authorizationService, nameof(authorizationService));

            _authorizationService = authorizationService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var anonymousPermissions = _authorizationService as IAnonymousPermissions;

            var permissions = anonymousPermissions != null
                ? new List<string>(anonymousPermissions.AnonymousPermissions())
                : _emptyAnonymousPermissions;

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
    }
}