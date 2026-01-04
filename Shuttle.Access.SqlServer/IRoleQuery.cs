namespace Shuttle.Access.SqlServer;

public interface IRoleQuery
{
    ValueTask<int> CountAsync(Models.Role.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Permission>> PermissionsAsync(Models.Role.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Role>> SearchAsync(Models.Role.Specification specification, CancellationToken cancellationToken = default);
}