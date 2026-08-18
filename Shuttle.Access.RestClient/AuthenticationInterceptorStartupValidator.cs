using Microsoft.Extensions.Hosting;

namespace Shuttle.Access.RestClient;

/// <summary>
///     The REST client calls the Shuttle.Access web API as this application, so it always needs a credential of its
///     own.  This fails the host at startup rather than on the first outbound call.
/// </summary>
internal sealed class AuthenticationInterceptorStartupValidator(IEnumerable<IAuthenticationInterceptor> authenticationInterceptors) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return authenticationInterceptors.Any()
            ? Task.CompletedTask
            : throw new InvalidOperationException(Resources.AuthenticationInterceptorException);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
