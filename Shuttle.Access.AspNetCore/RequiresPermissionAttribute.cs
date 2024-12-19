using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class RequiresPermissionAttribute : TypeFilterAttribute
{
    public RequiresPermissionAttribute(string permission) : base(typeof(RequiresPermission))
    {
        Arguments = new object[] { permission };
    }

    private class RequiresPermission : IAuthorizationFilter
    {
        private readonly IAccessService _accessService;
        private readonly string _permission;

        public RequiresPermission(IAccessService accessService, string permission)
        {
            Guard.AgainstNull(accessService);
            Guard.AgainstNullOrEmptyString(permission);

            _accessService = accessService;
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var sessionTokenResult = context.HttpContext.GetAccessSessionToken();

            if (!sessionTokenResult.Ok)
            {
                SetUnauthorized(context);
                return;
            }

            if (!_accessService.HasPermission(sessionTokenResult.SessionToken, _permission))
            {
                SetUnauthorized(context);
            }
        }

        private static void SetUnauthorized(AuthorizationFilterContext context)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}