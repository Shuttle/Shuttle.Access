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

    private class RequiresPermission(ISessionService sessionService, string permission) : IAuthorizationFilter
    {
        private readonly string _permission = Guard.AgainstEmpty(permission);
        private readonly ISessionService _sessionService = Guard.AgainstNull(sessionService);

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var tenantId = context.HttpContext.FindTenantId();
            var identityId = context.HttpContext.FindIdentityId();

            if (tenantId == null || identityId == null)
            {
                SetUnauthorized(context);
                return;
            }

            if (!_sessionService.HasPermissionAsync(tenantId.Value, identityId.Value, _permission).GetAwaiter().GetResult())
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