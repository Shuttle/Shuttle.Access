using Shuttle.Access.Query;

namespace Shuttle.Access.SqlServer;

public interface IIdentityQuery
{
    ValueTask<int> AdministratorCountAsync(CancellationToken cancellationToken = default);
    ValueTask<int> CountAsync(IdentitySpecification specification, CancellationToken cancellationToken = default);
    ValueTask<Guid> IdAsync(string identityName, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Permission>> PermissionsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> RoleIdsAsync(IdentitySpecification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> TenantIdsAsync(IdentitySpecification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Identity>> SearchAsync(IdentitySpecification specification, CancellationToken cancellationToken = default);
}