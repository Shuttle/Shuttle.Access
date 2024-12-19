using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.DataAccess;

public interface IPermissionQuery
{
    ValueTask<bool> ContainsAsync(Query.Permission.Specification specification, CancellationToken cancellationToken = default);
    ValueTask<int> CountAsync(Query.Permission.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Messages.v1.Permission>> SearchAsync(Query.Permission.Specification specification, CancellationToken cancellationToken = default);
}