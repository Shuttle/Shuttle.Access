using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.DataAccess;

public interface IRoleQuery
{
    ValueTask<int> CountAsync(Role.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Messages.v1.Permission>> PermissionsAsync(Role.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Messages.v1.Role>> SearchAsync(Role.Specification specification, CancellationToken cancellationToken = default);
}