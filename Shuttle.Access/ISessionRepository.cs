namespace Shuttle.Access;

public interface ISessionRepository
{
    Task<Session?> FindAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Session?> FindAsync(byte[] token, CancellationToken cancellationToken = default);
    Task<Session?> FindAsync(Guid tenantId, string identityName, CancellationToken cancellationToken = default);
    Task<Session?> FindAsync(Guid tenantId, Guid identityId, CancellationToken cancellationToken = default);
    Task RemoveAllAsync(CancellationToken cancellationToken = default);
    ValueTask<bool> RemoveAsync(Guid identityId, CancellationToken cancellationToken = default);
    Task SaveAsync(Session session, CancellationToken cancellationToken = default);
}