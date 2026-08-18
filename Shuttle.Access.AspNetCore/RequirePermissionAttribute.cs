using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

/// <summary>
///     The MVC controller equivalent of <c>RouteHandlerBuilder.RequirePermission(string)</c>.  The session has already
///     been established by <see cref="AccessAuthenticationHandler" />, so this only applies the requirement.
/// </summary>
public class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string permission) : base(typeof(RequiresPermission))
    {
        Arguments = [permission];
    }

    private class RequiresPermission(ISessionContext sessionContext, string permission) : IAuthorizationFilter
    {
        private readonly string _permission = Guard.AgainstEmpty(permission);
        private readonly ISessionContext _sessionContext = Guard.AgainstNull(sessionContext);

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            Guard.AgainstNull(context);

            if (!_sessionContext.IsAuthorized)
            {
                context.Result = new UnauthorizedResult();

                return;
            }

            if (!_sessionContext.HasPermission(_permission))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
