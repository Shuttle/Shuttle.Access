using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.DataAccess
{
    public interface IRoleQuery
    {
        Task<IEnumerable<Messages.v1.Role>> SearchAsync(RoleSpecification specification, CancellationToken cancellationToken = default);
        Task<IEnumerable<Messages.v1.Permission>> PermissionsAsync(RoleSpecification specification, CancellationToken cancellationToken = default);
        ValueTask<int> CountAsync(RoleSpecification specification, CancellationToken cancellationToken = default);
    }
}