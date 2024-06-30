using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.DataAccess
{
    public interface IRoleQuery
    {
        Task<IEnumerable<Query.Role>> SearchAsync(Query.Role.Specification specification, CancellationToken cancellationToken = default);
        Task<IEnumerable<Query.Permission>> PermissionsAsync(Query.Role.Specification specification, CancellationToken cancellationToken = default);
        ValueTask<int> CountAsync(Query.Role.Specification specification, CancellationToken cancellationToken = default);
    }
}