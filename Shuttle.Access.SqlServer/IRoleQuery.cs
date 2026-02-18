using Shuttle.Access.Query;

namespace Shuttle.Access.SqlServer;

public interface IRoleQuery
{
    ValueTask<int> CountAsync(RoleSpecification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Permission>> PermissionsAsync(RoleSpecification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Role>> SearchAsync(RoleSpecification specification, CancellationToken cancellationToken = default);
}