using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access;

public interface ISessionRepository
{
    Task<Session?> FindAsync(Guid token, CancellationToken cancellationToken = default);
    ValueTask<bool> RemoveAsync(Guid token, CancellationToken cancellationToken = default);
    Task SaveAsync(Session session, CancellationToken cancellationToken = default);
}