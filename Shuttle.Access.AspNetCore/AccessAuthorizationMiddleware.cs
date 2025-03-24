using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationMiddleware : IMiddleware
{
    private readonly ISessionCache _sessionCache;
    private readonly StringValues _wwwAuthenticate;

    public AccessAuthorizationMiddleware(IOptions<AccessOptions> accessOptions, ISessionCache sessionCache)
    {
        var options = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);

        _sessionCache = Guard.AgainstNull(sessionCache);

        _wwwAuthenticate = $"Shuttle.Access realm=\"{options.Realm}\", token=\"GUID\"; Bearer realm=\"{options.Realm}\"";
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();

        var permissionRequirement = endpoint?.Metadata.GetMetadata<AccessPermissionRequirement>();
        var sessionRequirement = endpoint?.Metadata.GetMetadata<AccessSessionRequirement>();

        if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized )
        {
            return;
        }

        if (permissionRequirement == null && sessionRequirement == null)
        {
            await next(context);

            return;
        }

        var sessionIdentityId = context.GetIdentityId();

        if (sessionIdentityId == null)
        {
            Unauthorized(context);
            return;
        }

        if (permissionRequirement != null &&
            !await _sessionCache.HasPermissionAsync(sessionIdentityId.Value, permissionRequirement.Permission))
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