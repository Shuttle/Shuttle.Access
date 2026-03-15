namespace Shuttle.Access;

public interface IIdentityQuery
{
    ValueTask<int> AdministratorCountAsync(Guid tenantId, CancellationToken cancellationToken = default);
    ValueTask<int> CountAsync(Query.Identity.Specification specification, CancellationToken cancellationToken = default);
    ValueTask<Guid> IdAsync(string identityName, CancellationToken cancellationToken = default);
    Task<IEnumerable<Query.Permission>> PermissionsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> RoleIdsAsync(Query.Identity.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> TenantIdsAsync(Query.Identity.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Query.Identity>> SearchAsync(Query.Identity.Specification specification, CancellationToken cancellationToken = default);
}