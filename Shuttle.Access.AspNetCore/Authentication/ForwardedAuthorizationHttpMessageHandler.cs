using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

/// <summary>
///     Copies the caller's `Authorization` and `Shuttle-Access-Tenant-Id` headers onto the outgoing request so that
///     the Shuttle.Access web API resolves the session for the *caller*.
/// </summary>
public class ForwardedAuthorizationHttpMessageHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    private static readonly string UserAgent = $"Shuttle.Access{(Assembly.GetExecutingAssembly().GetName().Version is { } version ? $"/{version.Major}.{version.Minor}.{version.Build}" : string.Empty)}";

    private readonly IHttpContextAccessor _httpContextAccessor = Guard.AgainstNull(httpContextAccessor);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Guard.AgainstNull(request).Headers.Add("User-Agent", UserAgent);

        var httpRequest = _httpContextAccessor.HttpContext?.Request;

        if (httpRequest != null)
        {
            if (httpRequest.Headers.TryGetValue("Authorization", out var authorizationValues) &&
                AuthenticationHeaderValue.TryParse(authorizationValues.ToString(), out var authenticationHeaderValue))
            {
                request.Headers.Authorization = authenticationHeaderValue;
            }

            if (httpRequest.Headers.TryGetValue(HttpRequestExtensions.TenantIdHeaderName, out var tenantIdValues) &&
                !string.IsNullOrWhiteSpace(tenantIdValues.FirstOrDefault()))
            {
                request.Headers.Add(HttpRequestExtensions.TenantIdHeaderName, tenantIdValues.First());
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
