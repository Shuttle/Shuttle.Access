using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public static class RoleQueryExtensions
{
    extension(IRoleQuery roleQuery)
    {
        public async ValueTask<bool> ContainsAsync(Models.Role.Specification specification, CancellationToken cancellationToken = default)
        {
            return await Guard.AgainstNull(roleQuery).CountAsync(specification, cancellationToken) > 0;
        }
    }
}