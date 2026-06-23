namespace Shuttle.Access;

public interface IIdentityQuery : IQuery<Query.Identity, Query.Identity.Specification>
{
    ValueTask<int> AdministratorCountAsync(Guid tenantId, CancellationToken cancellationToken = default);
    ValueTask<Guid> IdAsync(string identityName, CancellationToken cancellationToken = default);
    Task<IEnumerable<Query.Session.SessionPermission>> PermissionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> RoleIdsAsync(Query.Identity.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> TenantIdsAsync(Query.Identity.Specification specification, CancellationToken cancellationToken = default);
}