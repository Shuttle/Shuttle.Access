using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public static class TenantQueryExtensions
{
    extension(ITenantQuery roleQuery)
    {
        public async ValueTask<bool> ContainsAsync(Models.Tenant.Specification specification, CancellationToken cancellationToken = default)
        {
            return await Guard.AgainstNull(roleQuery).CountAsync(specification, cancellationToken) > 0;
        }
    }
}