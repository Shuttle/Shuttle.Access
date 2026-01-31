using Shuttle.Access.Query;

namespace Shuttle.Access.SqlServer;

public interface IRoleQuery
{
    ValueTask<int> CountAsync(RoleSpecification roleSpecification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Permission>> PermissionsAsync(RoleSpecification roleSpecification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Role>> SearchAsync(RoleSpecification roleSpecification, CancellationToken cancellationToken = default);
}