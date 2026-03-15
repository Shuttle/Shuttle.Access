namespace Shuttle.Access;

public interface IRoleQuery : IQuery<Query.Role, Query.Role.Specification>
{
    Task<IEnumerable<Query.Permission>> PermissionsAsync(Query.Role.Specification specification, CancellationToken cancellationToken = default);
}