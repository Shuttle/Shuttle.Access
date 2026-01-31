using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationMiddleware : IMiddleware
{
    private readonly ISessionService _sessionService;
    private readonly StringValues _wwwAuthenticate;

    public AccessAuthorizationMiddleware(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, ISessionService sessionService)
    {
        var options = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);

        _sessionService = Guard.AgainstNull(sessionService);

        _wwwAuthenticate = $"Shuttle.Access realm=\"{options.Realm}\", token=\"GUID\"; Bearer realm=\"{options.Realm}\"";
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();

        var permissionRequirement = endpoint?.Metadata.GetMetadata<AccessPermissionRequirement>();
        var sessionRequirement = endpoint?.Metadata.GetMetadata<AccessSessionRequirement>();

        if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
        {
            return;
        }

        if (permissionRequirement == null && sessionRequirement == null)
        {
            await next(context);

            return;
        }

        var tenantId = context.FindTenantId();
        var identityId = context.FindIdentityId();

        if (tenantId == null || identityId == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.Append("WWW-Authenticate", _wwwAuthenticate);
            return;
        }

        if (permissionRequirement != null &&
            !((await _sessionService.FindAsync(new() { TenantId = tenantId.Value, IdentityId =  identityId.Value}))?.HasPermission(permissionRequirement.Permission) ?? false))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await next(context);
    }
}