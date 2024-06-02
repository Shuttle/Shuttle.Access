using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access
{
    public interface ISessionService
    {
        Task<RegisterSessionResult> RegisterAsync(string identityName, Guid requesterToken, CancellationToken cancellationToken = default);
        Task<RegisterSessionResult> RegisterAsync(string identityName, string password, Guid token, CancellationToken cancellationToken = default);
        ValueTask<bool> RemoveAsync(Guid token, CancellationToken cancellationToken = default);
        ValueTask<bool> RemoveAsync(string identityName, CancellationToken cancellationToken = default);
        Task RefreshAsync(Guid token, CancellationToken cancellationToken = default);

        event EventHandler<SessionOperationEventArgs> SessionOperation;
    }
}