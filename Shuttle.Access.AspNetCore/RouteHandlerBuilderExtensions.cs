using Microsoft.AspNetCore.Builder;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

public static class RouteHandlerBuilderExtensions
{
    extension(RouteHandlerBuilder builder)
    {
        public RouteHandlerBuilder RequirePermission(string permission)
        {
            return Guard.AgainstNull(builder).WithMetadata(new AccessPermissionRequirement(Guard.AgainstEmpty(permission)));
        }

        public RouteHandlerBuilder RequireSession()
        {
            return Guard.AgainstNull(builder).WithMetadata(new AccessSessionRequirement());
        }
    }
}