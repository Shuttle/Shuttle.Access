using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Access.Messages.v1;
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
            
            if (!_sessionService.FindAsync(new() { TenantId = tenantId.Value, IdentityId = identityId.Value }).GetAwaiter().GetResult()?.HasPermission(_permission) ?? false)
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