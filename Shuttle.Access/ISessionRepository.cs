using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access;

public interface ISessionRepository
{
    Task<Session?> FindAsync(byte[] token, CancellationToken cancellationToken = default);
    Task<Session?> FindAsync(string identityName, CancellationToken cancellationToken = default);
    Task<Session?> FindAsync(Guid identityId, CancellationToken cancellationToken = default);
    ValueTask<bool> RemoveAsync(Guid identityId, CancellationToken cancellationToken = default);
    Task RemoveAllAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(Session session, CancellationToken cancellationToken = default);
}