using Shuttle.Access.Query;

namespace Shuttle.Access.SqlServer;

public interface ITenantQuery
{
    ValueTask<int> CountAsync(TenantSpecification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Tenant>> SearchAsync(TenantSpecification specification, CancellationToken cancellationToken = default);
}