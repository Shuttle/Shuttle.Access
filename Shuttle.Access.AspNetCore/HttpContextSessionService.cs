using Microsoft.AspNetCore.Http;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class HttpContextSessionService : IHttpContextSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISessionService _sessionService;

    public HttpContextSessionService(ISessionService sessionService, IHttpContextAccessor httpContextAccessor)
    {
        _sessionService = Guard.AgainstNull(sessionService);
        _httpContextAccessor = Guard.AgainstNull(httpContextAccessor);
    }

    public async ValueTask<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default)
    {
        var httpContext = Guard.AgainstNull(_httpContextAccessor.HttpContext);

        var identityId = httpContext.GetIdentityId();

        if (identityId == null)
        {
            throw new ApplicationException(Resources.HttpContextIdentityIdNotFoundException);
        }

        return await _sessionService.HasPermissionAsync(identityId.Value, permission, cancellationToken);
    }
}