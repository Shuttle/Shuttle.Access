namespace Shuttle.Access;

public interface ISessionQuery : IQuery<Query.Session, Query.Session.Specification>
{
    ValueTask<int> RemoveAsync(Query.Session.Specification specification, CancellationToken cancellationToken = default);
    Task SaveAsync(Query.Session session, CancellationToken cancellationToken = default);
}