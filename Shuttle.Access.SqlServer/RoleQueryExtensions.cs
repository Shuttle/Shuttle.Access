using Shuttle.Access.Query;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public static class RoleQueryExtensions
{
    extension(IRoleQuery roleQuery)
    {
        public async ValueTask<bool> ContainsAsync(RoleSpecification roleSpecification, CancellationToken cancellationToken = default)
        {
            return await Guard.AgainstNull(roleQuery).CountAsync(roleSpecification, cancellationToken) > 0;
        }
    }
}