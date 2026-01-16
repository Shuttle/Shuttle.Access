namespace Shuttle.Access.SqlServer;

public interface ITenantQuery
{
    Task<IEnumerable<Models.Tenant>> SearchAsync(Models.Tenant.Specification specification, CancellationToken cancellationToken = default);
}