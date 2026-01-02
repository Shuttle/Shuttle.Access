namespace Shuttle.Access.Data;

public interface IIdentityQuery
{
    ValueTask<int> AdministratorCountAsync(CancellationToken cancellationToken = default);
    ValueTask<int> CountAsync(Models.Identity.Specification specification, CancellationToken cancellationToken = default);
    ValueTask<Guid> IdAsync(string identityName, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Permission>> PermissionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> RoleIdsAsync(Models.Identity.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Identity>> SearchAsync(Models.Identity.Specification specification, CancellationToken cancellationToken = default);
}