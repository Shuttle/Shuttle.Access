using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class RequiresSessionAttribute : TypeFilterAttribute
{
    public RequiresSessionAttribute() : base(typeof(RequiresSession))
    {
        Arguments = [];
    }

    private class RequiresSession : IAuthorizationFilter
    {
        private readonly IAccessService _accessService;

        public RequiresSession(IAccessService accessService)
        {
            Guard.AgainstNull(accessService);

            _accessService = accessService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var sessionTokenResult = context.HttpContext.GetAccessSessionToken();

            if (!sessionTokenResult.Ok)
            {
                SetUnauthorized(context);
                return;
            }

            if (!_accessService.ContainsAsync(sessionTokenResult.SessionToken).GetAwaiter().GetResult())
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