using Microsoft.AspNetCore.Builder;

namespace Shuttle.Access.AspNetCore;

public static class ApplicationBuilderExtensions
{
    extension(IApplicationBuilder builder)
    {
        public IApplicationBuilder UseAccessAuthorization()
        {
            return builder.UseMiddleware<AccessAuthorizationMiddleware>();
        }
    }
}