using Shuttle.Access.Query;

namespace Shuttle.Access.SqlServer;

public interface IIdentityQuery
{
    ValueTask<int> AdministratorCountAsync(CancellationToken cancellationToken = default);
    ValueTask<int> CountAsync(IdentitySpecification identitySpecification, CancellationToken cancellationToken = default);
    ValueTask<Guid> IdAsync(string identityName, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Permission>> PermissionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> RoleIdsAsync(IdentitySpecification identitySpecification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Identity>> SearchAsync(IdentitySpecification identitySpecification, CancellationToken cancellationToken = default);
}