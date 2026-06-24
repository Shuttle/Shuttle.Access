using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shuttle.Contract;
using System.Net.Http.Headers;
using System.Security.Authentication;

namespace Shuttle.Access.RestClient;

public class BearerAuthenticationInterceptor(IOptions<BearerAuthenticationInterceptorOptions> bearerAuthenticationInterceptorOptions, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
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
            var httpRequest = httpContextAccessor.HttpContext?.Request;

            if (httpRequest?.Headers.ContainsKey("Shuttle-Access-Tenant-Id") ?? false)
            {
                httpRequestMessage.Headers.Add("Shuttle-Access-Tenant-Id", httpRequest.Headers["Shuttle-Access-Tenant-Id"].First());
            }
            else if (_bearerAuthenticationInterceptorOptions.TenantId.HasValue)
            {
                httpRequestMessage.Headers.Add("Shuttle-Access-Tenant-Id", $"{_bearerAuthenticationInterceptorOptions.TenantId.Value:D}");
            }

            if (httpRequest?.Headers.ContainsKey("Shuttle-Access-Application") ?? false)
            {
                httpRequestMessage.Headers.Add("Shuttle-Access-Application", httpRequest.Headers["Shuttle-Access-Application"].First());
            }
            else if (!string.IsNullOrWhiteSpace(_bearerAuthenticationInterceptorOptions.Application))
            {
                httpRequestMessage.Headers.Add("Shuttle-Access-Application", _bearerAuthenticationInterceptorOptions.Application);
            }

            if ((httpRequest?.Headers.TryGetValue("Authorization", out var authorizationValues) ?? false) &&
                AuthenticationHeaderValue.TryParse(authorizationValues.ToString(), out var authenticationHeaderValue))
            {
                httpRequestMessage.Headers.Authorization = authenticationHeaderValue;
                return;
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