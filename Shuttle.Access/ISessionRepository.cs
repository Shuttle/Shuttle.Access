namespace Shuttle.Access;

public interface ISessionRepository
{
    Task<IEnumerable<Session>> SearchAsync(Query.Session.Specification specification, CancellationToken cancellationToken = default);
    ValueTask<int> RemoveAsync(Query.Session.Specification specification, CancellationToken cancellationToken = default);
    Task SaveAsync(Session session, CancellationToken cancellationToken = default);
    Task<Session> GetAsync(Guid id, CancellationToken cancellationToken = default);
}