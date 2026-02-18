using Shuttle.Access.Query;

namespace Shuttle.Access.SqlServer;

public interface ISessionQuery
{
    ValueTask<int> CountAsync(SessionSpecification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Session>> SearchAsync(SessionSpecification specification, CancellationToken cancellationToken = default);
}