using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationMiddleware(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, ISessionContext sessionContext, ISessionService sessionService, ILogger<AccessAuthorizationMiddleware>? logger = null) : IMiddleware
{
    private readonly AccessAuthorizationOptions _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
    private readonly ILogger<AccessAuthorizationMiddleware> _logger = logger ?? NullLogger<AccessAuthorizationMiddleware>.Instance;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
        {
            return;
        }

        var identityId = context.FindIdentityId();
        var tenantId = context.FindTenantId();

        if (tenantId != null && identityId != null)
        {
            var specification = new Session.Specification().WithTenantId(tenantId.Value).WithIdentityId(identityId.Value);

            sessionContext.Session = await Guard.AgainstNull(sessionService).FindAsync(specification);

            if (sessionContext.Session == null)
            {
                LogMessage.NoActiveSession(_logger, identityId.Value, tenantId);
            }
        }

        var endpoint = context.GetEndpoint();

        var permissionRequirement = endpoint?.Metadata.GetMetadata<AccessPermissionRequirement>();
        var sessionRequirement = endpoint?.Metadata.GetMetadata<AccessSessionRequirement>();

        if (permissionRequirement == null && sessionRequirement == null)
        {
            await next(context);

            return;
        }

        if (tenantId == null || identityId == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.Append("WWW-Authenticate", $"Shuttle.Access realm=\"{_accessAuthorizationOptions.Realm}\", token=\"GUID\"; Bearer realm=\"{_accessAuthorizationOptions.Realm}\"");
            return;
        }

        if (permissionRequirement != null &&
            sessionContext.Session != null &&
            !sessionContext.Session.HasPermission(permissionRequirement.Permission))
        {
            LogMessage.PermissionDenied(_logger, sessionContext.Session.IdentityName, sessionContext.Session.TenantName, permissionRequirement.Permission);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await next(context);
    }
}