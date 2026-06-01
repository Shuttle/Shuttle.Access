namespace Shuttle.Access;

public interface ISessionService
{
    Task<Query.Session?> GetSelfAsync(CancellationToken cancellationToken = default);
    Task<Query.Session?> FindAsync(Query.Session.Specification specification, CancellationToken cancellationToken = default);
}