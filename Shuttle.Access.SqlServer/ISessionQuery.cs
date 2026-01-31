using Shuttle.Access.Query;

namespace Shuttle.Access.SqlServer;

public interface ISessionQuery
{
    ValueTask<int> CountAsync(SessionSpecification sessionSpecification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Session>> SearchAsync(SessionSpecification sessionSpecification, CancellationToken cancellationToken = default);
}