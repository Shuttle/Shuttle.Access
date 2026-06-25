using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shuttle.Contract;
using System.Security.Authentication;
using Shuttle.Access.AspNetCore;

namespace Shuttle.Access.RestClient;

public class BearerAuthenticationInterceptor(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, IOptions<BearerAuthenticationInterceptorOptions> bearerAuthenticationInterceptorOptions, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
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
            if (accessAuthorizationOptions.Value.PassThrough)
            {
                throw new ApplicationException(string.Format(Resources.PassThroughNotSupportedOnInterceptor, nameof(BearerAuthenticationInterceptor)));
            }

            var httpRequest = httpContextAccessor.HttpContext?.Request;

            if (httpRequest?.Headers.ContainsKey("Shuttle-Access-Tenant-Id") ?? false)
            {
                httpRequestMessage.Headers.Add("Shuttle-Access-Tenant-Id", httpRequest.Headers["Shuttle-Access-Tenant-Id"].First());
            }
            else if (_bearerAuthenticationInterceptorOptions.TenantId.HasValue)
            {
                httpRequestMessage.Headers.Add("Shuttle-Access-Tenant-Id", $"{_bearerAuthenticationInterceptorOptions.TenantId.Value:D}");
            }

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
        }
        finally
        {
            _lock.Release();
        }
    }
}