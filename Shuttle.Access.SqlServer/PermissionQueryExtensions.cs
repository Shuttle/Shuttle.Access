using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public static class PermissionQueryExtensions
{
    extension(IPermissionQuery permissionQuery)
    {
        public async ValueTask<bool> ContainsAsync(Models.Permission.Specification specification, CancellationToken cancellationToken = default)
        {
            return await Guard.AgainstNull(permissionQuery).CountAsync(specification, cancellationToken) > 0;
        }
    }
}