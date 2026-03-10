using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using System.Net;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationMiddleware(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, ISessionContext sessionContext, ISessionService sessionService, ILogger<AccessAuthorizationMiddleware>? logger = null) : IMiddleware
{
    private readonly AccessAuthorizationOptions _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
    private readonly ILogger<AccessAuthorizationMiddleware> _logger = logger ?? NullLogger<AccessAuthorizationMiddleware>.Instance;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var identityId = context.FindIdentityId();
        var tenantId = context.FindTenantId();

        if (identityId != null)
        {
            var specification = new Messages.v1.Session.Specification()
            {
                TenantId = tenantId,
                IdentityId = identityId.Value
            };

            if (!tenantId.HasValue)
            {
                specification.HasNullTenantId = true;
            }

            sessionContext.Session = await Guard.AgainstNull(sessionService).FindAsync(specification);

            if (sessionContext.Session == null)
            {
                LogMessage.NoActiveSession(logger, identityId.Value, tenantId);
            }
        }

        if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
        {
            return;
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
            LogMessage.PermissionDenied(logger, sessionContext.Session.IdentityName, sessionContext.Session.TenantName, permissionRequirement.Permission);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await next(context);
    }
}