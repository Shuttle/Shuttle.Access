using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.Tests.Integration
{
    public class SessionService : ISessionService
    {
        public Task<RegisterSessionResult> RegisterAsync(string identityName, Guid requesterToken, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<RegisterSessionResult> RegisterAsync(string identityName, string password, Guid token, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(RegisterSessionResult.Success(identityName ?? "identity", Guid.Empty.Equals(token) ? Guid.NewGuid() : token, DateTime.MaxValue, new[] { "*" }));
        }

        public async ValueTask<bool> RemoveAsync(Guid token, CancellationToken cancellationToken = default)
        {
            return await ValueTask.FromResult(true);
        }

        public async ValueTask<bool> RemoveAsync(string identityName, CancellationToken cancellationToken = default)
        {
            return await ValueTask.FromResult(true);
        }

        public async Task RefreshAsync(Guid token, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        public event EventHandler<SessionOperationEventArgs> SessionOperation = delegate
        {
        };
    }
}