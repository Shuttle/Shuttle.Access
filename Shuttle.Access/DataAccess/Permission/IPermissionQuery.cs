using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.DataAccess;

public interface IPermissionQuery
{
    ValueTask<bool> ContainsAsync(Permission.Specification specification, CancellationToken cancellationToken = default);
    ValueTask<int> CountAsync(Permission.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> SearchAsync(Permission.Specification specification, CancellationToken cancellationToken = default);
}