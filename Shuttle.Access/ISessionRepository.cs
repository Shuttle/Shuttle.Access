using Shuttle.Access.Query;

namespace Shuttle.Access;

public interface ISessionRepository
{
    Task<IEnumerable<Session>> SearchAsync(SessionSpecification specification, CancellationToken cancellationToken = default);
    ValueTask<int> RemoveAsync(SessionSpecification specification, CancellationToken cancellationToken = default);
    Task SaveAsync(Session session, CancellationToken cancellationToken = default);
    Task<Session> GetAsync(Guid id, CancellationToken cancellationToken = default);
}