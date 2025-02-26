using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access;

public interface IAccessService
{
    ValueTask<bool> ContainsAsync(Guid token, CancellationToken cancellationToken = default);
    Task FlushAsync(CancellationToken cancellationToken = default);
    ValueTask<bool> HasPermissionAsync(Guid token, string permission, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid token, CancellationToken cancellationToken = default);
    Task<Session?> FindSessionAsync(Guid token, CancellationToken cancellationToken = default);
}