using Microsoft.AspNetCore.Builder;

namespace Shuttle.Access.AspNetCore;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAccessAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AccessAuthorizationMiddleware>();
    }
}