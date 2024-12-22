using Microsoft.AspNetCore.Http;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationMiddleware : IMiddleware
{
    public static readonly string AuthorizationScheme = "Bearer";
    private static readonly char[] Space = { ' ' };
    private readonly IAccessService _accessService;

    public AccessAuthorizationMiddleware(IAccessService accessService)
    {
        _accessService = Guard.AgainstNull(accessService);
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();

        var permissionRequirement = endpoint?.Metadata.GetMetadata<AccessPermissionRequirement>();
        var sessionRequirement = endpoint?.Metadata.GetMetadata<AccessSessionRequirement>();

        if (permissionRequirement == null && sessionRequirement == null)
        {
            await next(context);

            return;
        }

        var headers = context.Request.Headers["Authorization"];

        if (headers.Count != 1)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            return;
        }

        var values = headers[0]!.Split(Space);

        if (values.Length != 2 ||
            !values[0].Equals(AuthorizationScheme) ||
            !Guid.TryParse(values[1], out var sessionToken))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            return;
        }

        if (permissionRequirement != null &&
            !await _accessService.HasPermissionAsync(sessionToken, permissionRequirement.Permission))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            return;
        }

        if (sessionRequirement != null &&
            !await _accessService.ContainsAsync(sessionToken))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            return;
        }

        await next(context);
    }
}