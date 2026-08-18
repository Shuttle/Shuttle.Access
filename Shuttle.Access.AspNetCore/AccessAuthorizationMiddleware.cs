using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

/// <summary>
///     Applies the <see cref="AccessSessionRequirement" /> / <see cref="AccessPermissionRequirement" /> endpoint
///     metadata.  The session has already been established by <see cref="AccessAuthenticationHandler" /> and is read
///     from the <see cref="ISessionContext" />.
/// </summary>
public class AccessAuthorizationMiddleware(ISessionContext sessionContext, ILogger<AccessAuthorizationMiddleware>? logger = null) : IMiddleware
{
    private readonly ILogger<AccessAuthorizationMiddleware> _logger = logger ?? NullLogger<AccessAuthorizationMiddleware>.Instance;
    private readonly ISessionContext _sessionContext = Guard.AgainstNull(sessionContext);

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = Guard.AgainstNull(context).GetEndpoint();

        var permissionRequirement = endpoint?.Metadata.GetMetadata<AccessPermissionRequirement>();
        var sessionRequirement = endpoint?.Metadata.GetMetadata<AccessSessionRequirement>();

        if (permissionRequirement == null && sessionRequirement == null)
        {
            await next(context);

            return;
        }

        if (!_sessionContext.IsAuthorized)
        {
            await context.ChallengeAsync();

            return;
        }

        if (permissionRequirement != null &&
            !_sessionContext.HasPermission(permissionRequirement.Permission))
        {
            LogMessage.PermissionDenied(_logger, _sessionContext.Session.IdentityName, $"{_sessionContext.TenantId:D}", permissionRequirement.Permission);

            await context.ForbidAsync();

            return;
        }

        await next(context);
    }
}
