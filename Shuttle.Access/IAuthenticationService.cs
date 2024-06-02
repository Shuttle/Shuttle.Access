using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateAsync(string identityName, string password, CancellationToken cancellationToken = default);

        event EventHandler<AuthenticationEventArgs> Authentication;
    }
}