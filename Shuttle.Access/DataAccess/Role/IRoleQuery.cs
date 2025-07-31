using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.DataAccess;

public interface IRoleQuery
{
    ValueTask<int> CountAsync(Role.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> PermissionsAsync(Role.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> SearchAsync(Role.Specification specification, CancellationToken cancellationToken = default);
}