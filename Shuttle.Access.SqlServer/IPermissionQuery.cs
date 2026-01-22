namespace Shuttle.Access.SqlServer;

public interface IPermissionQuery
{
    ValueTask<int> CountAsync(Models.Permission.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Permission>> SearchAsync(Models.Permission.Specification specification, CancellationToken cancellationToken = default);
}