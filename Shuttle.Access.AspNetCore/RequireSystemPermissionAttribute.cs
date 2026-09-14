using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Shuttle.Access.Query;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

/// <summary>
///     The MVC controller equivalent of <c>RouteHandlerBuilder.RequireSystemPermission(string)</c>.  The session has
///     already been established by <see cref="AccessAuthenticationHandler" />, so this only applies the requirement.
/// </summary>
public class RequireSystemPermissionAttribute : TypeFilterAttribute
{
    public RequireSystemPermissionAttribute(string permission) : base(typeof(RequiresSystemPermission))
    {
        Arguments = [permission];
    }

    private class RequiresSystemPermission(ISessionContext sessionContext, IOptions<AccessOptions> accessOptions, string permission) : IAuthorizationFilter
    {
        private readonly AccessOptions _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
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

            if (!_sessionContext.Session.HasPermission(_accessOptions.SystemTenantId, _permission))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
