using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationMiddleware : IMiddleware
{
    public static readonly string AuthorizationScheme = "Shuttle.Access";
    private readonly IAccessService _accessService;
    private readonly StringValues _wwwAuthenticate;


    public AccessAuthorizationMiddleware(IOptions<AccessOptions> accessOptions, IAccessService accessService)
    {
        var options = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);

        _accessService = Guard.AgainstNull(accessService);

        _wwwAuthenticate = $"Shuttle.Access realm=\"{options.Realm}\", token=\"GUID\"; Bearer realm=\"{options.Realm}\"";
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

        var sessionTokenResult = context.GetAccessSessionToken();

        if (!sessionTokenResult.Ok)
        {
            Unauthorized(context);
            return;
        }

        if (!await _accessService.ContainsAsync(sessionTokenResult.SessionToken))
        {
            Unauthorized(context);
            return;
        }

        if (permissionRequirement != null &&
            !await _accessService.HasPermissionAsync(sessionTokenResult.SessionToken, permissionRequirement.Permission))
        {
            Unauthorized(context);
            return;
        }

        await next(context);
    }

    private void Unauthorized(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers.Append("WWW-Authenticate", _wwwAuthenticate);
    }
}