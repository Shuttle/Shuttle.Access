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

    private class RequiresSession : IAuthorizationFilter
    {
        private readonly ISessionCache _sessionCache;

        public RequiresSession(ISessionCache sessionCache)
        {
            _sessionCache = Guard.AgainstNull(sessionCache);
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var sessionIdentityId = context.HttpContext.GetIdentityId();

            if (sessionIdentityId == null)
            {
                SetUnauthorized(context);
                return;
            }

            if (_sessionCache.FindAsync(sessionIdentityId.Value).GetAwaiter().GetResult() == null)
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