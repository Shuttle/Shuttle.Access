using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

public class RequireSessionAttribute : TypeFilterAttribute
{
    public RequireSessionAttribute() : base(typeof(RequiresSession))
    {
        Arguments = [];
    }

    private class RequiresSession(ISessionService sessionService) : IAsyncAuthorizationFilter
    {
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

            if (await _sessionService.FindAsync(new Query.Session.Specification().AddId(sessionId.Value)) == null)
            {
                SetUnauthorized(context);
            }
        }
    }
}