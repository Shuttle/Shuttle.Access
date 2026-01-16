using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class RequireSessionAttribute : TypeFilterAttribute
{
    public RequireSessionAttribute() : base(typeof(RequiresSession))
    {
        Arguments = [];
    }

    private class RequiresSession(ISessionService sessionService) : IAuthorizationFilter
    {
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

            if (_sessionService.FindAsync(tenantId.Value, identityId.Value).GetAwaiter().GetResult() == null)
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