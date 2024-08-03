using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess.Query;

namespace Shuttle.Access.DataAccess
{
    public interface IRoleQuery
    {
        Task<IEnumerable<Messages.v1.Role>> SearchAsync(Query.Role.Specification specification, CancellationToken cancellationToken = default);
        Task<IEnumerable<Messages.v1.Permission>> PermissionsAsync(Query.Role.Specification specification, CancellationToken cancellationToken = default);
        ValueTask<int> CountAsync(Query.Role.Specification specification, CancellationToken cancellationToken = default);
    }
}