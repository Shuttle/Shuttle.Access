using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess.Query;

namespace Shuttle.Access.DataAccess
{
    public interface IPermissionQuery
    {
        Task<IEnumerable<Messages.v1.Permission>> SearchAsync(Query.Permission.Specification specification, CancellationToken cancellationToken = default);
        ValueTask<int> CountAsync(Query.Permission.Specification specification, CancellationToken cancellationToken = default);
        ValueTask<bool> ContainsAsync(Query.Permission.Specification specification, CancellationToken cancellationToken = default);
    }
}