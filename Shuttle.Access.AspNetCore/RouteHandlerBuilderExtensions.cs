using Microsoft.AspNetCore.Builder;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, string permission)
    {
        return Guard.AgainstNull(builder, nameof(builder)).WithMetadata(new AccessPermissionRequirement(Guard.AgainstNullOrEmptyString(permission, nameof(permission))));
    }

    public static RouteHandlerBuilder RequireSession(this RouteHandlerBuilder builder)
    {
        return Guard.AgainstNull(builder, nameof(builder)).WithMetadata(new AccessSessionRequirement());
    }
}