namespace Shuttle.Access;

public interface ISessionService
{
    Task AddAsync(Guid? token, Messages.v1.Session session, CancellationToken cancellationToken = default);
    Task<Messages.v1.Session?> FindAsync(Guid tenantId, Guid identityId, CancellationToken cancellationToken = default);
    Task<Messages.v1.Session?> FindAsync(Guid tenantId, string identityName, CancellationToken cancellationToken = default);
    Task<Messages.v1.Session?> FindAsync(Guid token, CancellationToken cancellationToken = default);
    Task FlushAsync(CancellationToken cancellationToken = default);
    Task FlushAsync(Guid identityId, CancellationToken cancellationToken = default);
    ValueTask<bool> HasPermissionAsync(Guid tenantId, Guid identityId, string permission, CancellationToken cancellationToken = default);
}