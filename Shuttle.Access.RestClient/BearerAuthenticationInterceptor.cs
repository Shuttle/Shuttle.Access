using System.Security.Authentication;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access.RestClient;

public class BearerAuthenticationInterceptor(IOptions<BearerAuthenticationInterceptorOptions> bearerAuthenticationInterceptorOptions, IServiceProvider serviceProvider)
    : IAuthenticationInterceptor
{
    private readonly BearerAuthenticationInterceptorOptions _bearerAuthenticationInterceptorOptions = Guard.AgainstNull(Guard.AgainstNull(bearerAuthenticationInterceptorOptions).Value);
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IServiceProvider _serviceProvider = Guard.AgainstNull(serviceProvider);

    public async Task ConfigureAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            BearerAuthenticationContext? authenticationContext = null;

            if (_bearerAuthenticationInterceptorOptions.GetBearerAuthenticationContextAsync != null)
            {
                authenticationContext = await _bearerAuthenticationInterceptorOptions.GetBearerAuthenticationContextAsync.Invoke(httpRequestMessage, _serviceProvider);
            }

            if (authenticationContext == null)
            {
                throw new AuthenticationException(Resources.BearerAuthenticationContextException);
            }

            httpRequestMessage.Headers.Authorization = new("Bearer", authenticationContext.Bearer);

            if (authenticationContext.TenantId.HasValue)
            {
            }

            httpRequestMessage.Headers.Authorization = new("Bearer", authenticationContext.Bearer);
        }
        finally
        {
            _lock.Release();
        }
    }
}