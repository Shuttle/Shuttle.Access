using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string permission) : base(typeof(RequiresPermission))
    {
        Arguments = [permission];
    }

    private class RequiresPermission : IAuthorizationFilter
    {
        private readonly string _permission;
        private readonly ISessionService _sessionService;

        public RequiresPermission(ISessionService sessionService, string permission)
        {
            _sessionService = Guard.AgainstNull(sessionService);
            _permission = Guard.AgainstEmpty(permission);
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var sessionIdentityId = context.HttpContext.GetIdentityId();

            if (sessionIdentityId == null)
            {
                SetUnauthorized(context);
                return;
            }

            if (!_sessionService.HasPermissionAsync(sessionIdentityId.Value, _permission).GetAwaiter().GetResult())
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