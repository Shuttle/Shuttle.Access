using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Access.Query;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

public class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string permission) : base(typeof(RequiresPermission))
    {
        Arguments = [permission];
    }

    private class RequiresPermission(ISessionService sessionService, string permission) : IAsyncAuthorizationFilter
    {
        private readonly string _permission = Guard.AgainstEmpty(permission);
        private readonly ISessionService _sessionService = Guard.AgainstNull(sessionService);

        private static void SetUnauthorized(AuthorizationFilterContext context)
        {
            context.Result = new UnauthorizedResult();
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var tenantId = context.HttpContext.FindTenantId();
            var sessionId = context.HttpContext.FindSessionsId();

            if (tenantId == null || sessionId == null)
            {
                SetUnauthorized(context);
                return;
            }

            if (!(await _sessionService.FindAsync(new Session.Specification().AddId(sessionId.Value)))?.HasPermission(tenantId.Value, _permission) ?? false)
            {
                SetUnauthorized(context);
            }
        }
    }
}