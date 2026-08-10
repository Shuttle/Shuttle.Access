using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

/// <summary>
///     The MVC controller equivalent of <c>RouteHandlerBuilder.RequireSession()</c>.  The session has already been
///     established by <see cref="AccessAuthenticationHandler" />, so this only applies the requirement.
/// </summary>
public class RequireSessionAttribute : TypeFilterAttribute
{
    public RequireSessionAttribute() : base(typeof(RequiresSession))
    {
        Arguments = [];
    }

    private class RequiresSession(ISessionContext sessionContext) : IAuthorizationFilter
    {
        private readonly ISessionContext _sessionContext = Guard.AgainstNull(sessionContext);

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!_sessionContext.IsAuthorized)
            {
                Guard.AgainstNull(context).Result = new UnauthorizedResult();
            }
        }
    }
}
