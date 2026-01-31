using Shuttle.Access.Query;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public static class PermissionQueryExtensions
{
    extension(IPermissionQuery permissionQuery)
    {
        public async ValueTask<bool> ContainsAsync(PermissionSpecification permissionSpecification, CancellationToken cancellationToken = default)
        {
            return await Guard.AgainstNull(permissionQuery).CountAsync(permissionSpecification, cancellationToken) > 0;
        }
    }
}