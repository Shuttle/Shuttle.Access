using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

/// <summary>
///     Asks the Shuttle.Access web API who the caller is, by forwarding the caller's `Authorization` header to
///     `GET /v1/sessions/self`.  The credential is never inspected here — Shuttle.Access is the only validator.
/// </summary>
/// <remarks>
///     This is the resolver used by every application other than the Shuttle.Access web API itself.
/// </remarks>
public class DelegatedSessionResolver(IOptions<AccessOptions> accessOptions, IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, IHttpClientFactory httpClientFactory, ILogger<DelegatedSessionResolver>? logger = null) : ISessionResolver
{
    public const string HttpClientName = "Shuttle.Access.Session";

    private const string SelfPath = "/v1/sessions/self";

    private readonly AccessAuthorizationOptions _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
    private readonly AccessOptions _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
    private readonly IHttpClientFactory _httpClientFactory = Guard.AgainstNull(httpClientFactory);
    private readonly ILogger<DelegatedSessionResolver> _logger = logger ?? NullLogger<DelegatedSessionResolver>.Instance;

    public async Task<SessionResolutionResult> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(Guard.AgainstNull(httpContext).Request.Headers.Authorization.FirstOrDefault()))
        {
            return SessionResolutionResult.None;
        }

        var tenantId = httpContext.Request.GetTenantId(_logger, _accessOptions.SystemTenantId);

        if (!tenantId.HasValue)
        {
            return SessionResolutionResult.Failure(Resources.InvalidTenantIdHeaderException);
        }

        var response = await _httpClientFactory.CreateClient(HttpClientName).GetAsync(SelfPath, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            LogMessage.InvalidAuthorizationHeader(_logger, $"The Shuttle.Access web API returned '{(int)response.StatusCode}' for '{SelfPath}'.");

            await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("Caller", "(caller)"), cancellationToken);

            return SessionResolutionResult.Failure(Access.Resources.InvalidAuthorizationHeader);
        }

        var content = await response.Content.ReadFromJsonAsync<WebApi.Contracts.v1.Session>(cancellationToken);

        if (content == null)
        {
            await _accessAuthorizationOptions.SessionUnavailable.InvokeAsync(new("Caller", "(caller)"), cancellationToken);

            return SessionResolutionResult.Failure(Access.Resources.InvalidAuthorizationHeader);
        }

        var session = SessionMapper.Map(content);

        await _accessAuthorizationOptions.SessionAvailable.InvokeAsync(new(session), cancellationToken);

        return SessionResolutionResult.Authenticated(session, tenantId.Value);
    }
}
