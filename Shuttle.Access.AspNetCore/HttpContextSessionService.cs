using Microsoft.AspNetCore.Http;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class HttpContextSessionService(ISessionService sessionService, IHttpContextAccessor httpContextAccessor)
    : IHttpContextSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor = Guard.AgainstNull(httpContextAccessor);
    private readonly ISessionService _sessionService = Guard.AgainstNull(sessionService);

    public async ValueTask<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default)
    {
        var httpContext = Guard.AgainstNull(_httpContextAccessor.HttpContext);

        var tenantId = httpContext.FindTenantId();
        var identityId = httpContext.FindIdentityId();

        if (tenantId == null || identityId == null)
        {
            throw new ApplicationException(Resources.HttpContextIdentityIdNotFoundException);
        }

        return (await _sessionService.FindAsync(new() { TenantId = tenantId.Value, IdentityId = identityId.Value }, cancellationToken))?.HasPermission(permission) ?? false;
    }
}