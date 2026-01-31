using Shuttle.Access.Query;

namespace Shuttle.Access.SqlServer;

public interface IPermissionQuery
{
    ValueTask<int> CountAsync(PermissionSpecification permissionSpecification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Permission>> SearchAsync(PermissionSpecification permissionSpecification, CancellationToken cancellationToken = default);
}