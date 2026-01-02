namespace Shuttle.Access.Data;

public interface IPermissionQuery
{
    ValueTask<bool> ContainsAsync(Models.Permission.Specification specification, CancellationToken cancellationToken = default);
    ValueTask<int> CountAsync(Models.Permission.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Permission>> SearchAsync(Models.Permission.Specification specification, CancellationToken cancellationToken = default);
}