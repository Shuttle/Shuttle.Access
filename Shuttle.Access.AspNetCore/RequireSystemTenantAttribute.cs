using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

/// <summary>
///     The MVC controller equivalent of <c>RouteHandlerBuilder.RequireSystemTenant()</c>.  The session has already
///     been established by <see cref="AccessAuthenticationHandler" />, so this only applies the requirement.
/// </summary>
public class RequireSystemTenantAttribute : TypeFilterAttribute
{
    public RequireSystemTenantAttribute() : base(typeof(RequiresSystemTenant))
    {
        Arguments = [];
    }

    private class RequiresSystemTenant(ISessionContext sessionContext, IOptions<AccessOptions> accessOptions) : IAuthorizationFilter
    {
        private readonly AccessOptions _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
        private readonly ISessionContext _sessionContext = Guard.AgainstNull(sessionContext);

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            Guard.AgainstNull(context);

            if (!_sessionContext.IsAuthorized)
            {
                context.Result = new UnauthorizedResult();

                return;
            }

            if (!_sessionContext.TenantId.Equals(_accessOptions.SystemTenantId))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
