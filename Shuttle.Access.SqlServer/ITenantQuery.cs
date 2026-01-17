namespace Shuttle.Access.SqlServer;

public interface ITenantQuery
{
    ValueTask<int> CountAsync(Models.Tenant.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Tenant>> SearchAsync(Models.Tenant.Specification specification, CancellationToken cancellationToken = default);
}