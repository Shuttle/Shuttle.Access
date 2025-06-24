namespace Shuttle.Access.AspNetCore;

public interface IHttpContextSessionService
{
    ValueTask<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default);
}