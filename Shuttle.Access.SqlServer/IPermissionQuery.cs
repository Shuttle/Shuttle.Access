using Shuttle.Access.Query;

namespace Shuttle.Access.SqlServer;

public interface IPermissionQuery
{
    ValueTask<int> CountAsync(PermissionSpecification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Permission>> SearchAsync(PermissionSpecification specification, CancellationToken cancellationToken = default);
}