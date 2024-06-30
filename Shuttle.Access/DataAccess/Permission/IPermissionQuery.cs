using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.DataAccess
{
    public interface IPermissionQuery
    {
        Task<IEnumerable<Messages.v1.Permission>> SearchAsync(PermissionSpecification specification, CancellationToken cancellationToken = default);
        ValueTask<int> CountAsync(PermissionSpecification specification, CancellationToken cancellationToken = default);
        ValueTask<bool> ContainsAsync(PermissionSpecification specification, CancellationToken cancellationToken = default);
    }
}