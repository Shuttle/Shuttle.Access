using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class BearerAuthenticationInterceptor : IAuthenticationInterceptor
{
    private readonly AccessAuthorizationOptions _accessAuthorizationOptions;
    private readonly AccessClientOptions _accessClientOptions;
    private readonly string _baseAddress;
    private readonly BearerAuthenticationInterceptorOptions _bearerAuthenticationInterceptorOptions;
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IJwtService _jwtService;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IServiceProvider _serviceProvider;
    private DateTimeOffset _tokenExpiryDate = DateTimeOffset.MinValue;
    private string _token = string.Empty;

    public BearerAuthenticationInterceptor(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, IOptions<AccessClientOptions> accessClientOptions, IOptions<BearerAuthenticationInterceptorOptions> bearerAuthenticationInterceptorOptions, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IJwtService jwtService, IServiceProvider serviceProvider)
    {
        _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
        _accessClientOptions = Guard.AgainstNull(Guard.AgainstNull(accessClientOptions).Value);
        _bearerAuthenticationInterceptorOptions = Guard.AgainstNull(Guard.AgainstNull(bearerAuthenticationInterceptorOptions).Value);
        _httpContextAccessor = Guard.AgainstNull(httpContextAccessor);
        _httpClient = Guard.AgainstNull(httpClient);
        _jwtService = Guard.AgainstNull(jwtService);
        _serviceProvider = Guard.AgainstNull(serviceProvider);

        _baseAddress = _accessClientOptions.BaseAddress;

        if (_baseAddress.EndsWith("/"))
        {
            _baseAddress = _baseAddress[..^1];
        }
    }

    public async Task ConfigureAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_tokenExpiryDate > DateTimeOffset.UtcNow.Add(_accessClientOptions.RenewToleranceTimeSpan))
            {
                httpRequestMessage.Headers.Add("Shuttle.Access", _token);
                return;
            }

            BearerAuthenticationContext? authenticationContext = null;

            if (_bearerAuthenticationInterceptorOptions.GetBearerAuthenticationContextAsync != null)
            {
                authenticationContext = await _bearerAuthenticationInterceptorOptions.GetBearerAuthenticationContextAsync.Invoke(httpRequestMessage, _serviceProvider);
            }

            if (_accessAuthorizationOptions.PassThrough)
            {
                if (authenticationContext != null)
                {
                    httpRequestMessage.Headers.Authorization = new("Bearer", authenticationContext.Bearer);

                    if (authenticationContext.TenantId.HasValue)
                    {
                        httpRequestMessage.Headers.Add("Shuttle-Access-Tenant-Id", authenticationContext.TenantId.Value.ToString("D"));
                    }

                    return;
                }

                var authorizationHeaderValue = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();

                if (authenticationContext == null || string.IsNullOrWhiteSpace(authorizationHeaderValue) || !authorizationHeaderValue.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase) || authorizationHeaderValue.Length < 8)
                {
                    throw new AuthenticationException(Resources.AuthorizationHeaderException);
                }

                var tenantIdHeaderValue = _httpContextAccessor.HttpContext?.Request.Headers["Shuttle-Access-Tenant-Id"].FirstOrDefault();

                if (tenantIdHeaderValue == null || !Guid.TryParse(tenantIdHeaderValue, out _))
                {
                    throw new AuthenticationException(Resources.TenantIdException);
                }

                httpRequestMessage.Headers.Authorization = new("Bearer", authorizationHeaderValue);
                httpRequestMessage.Headers.Add("Shuttle-Access-Tenant-Id", tenantIdHeaderValue);

                return;
            }

            if (authenticationContext == null)
            {
                throw new AuthenticationException(Resources.BearerAuthenticationContextException);
            }

            var identityName = await _jwtService.GetIdentityNameAsync(authenticationContext.Bearer);

            if (string.IsNullOrWhiteSpace(identityName))
            {
                throw new AuthenticationException(Access.Resources.IdentityNameClaimNotFound);
            }

            var requestData = new
            {
                IdentityName = identityName
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseAddress}/v1/sessions");

            request.Content = content;
            request.Headers.Authorization = new("Bearer", authenticationContext.Bearer);

            if (authenticationContext.TenantId.HasValue)
            {
                request.Headers.Add("Shuttle-Access-Tenant-Id", authenticationContext.TenantId.Value.ToString("D"));
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new AuthenticationException(response.ReasonPhrase);
            }

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            var sessionResponse = JsonSerializer.Deserialize<SessionResponse>(responseString, _jsonSerializerOptions);

            if (sessionResponse == null)
            {
                throw new AuthenticationException();
            }

            _token = $"token={sessionResponse.Token:D}";
            _tokenExpiryDate = sessionResponse.ExpiryDate;

            httpRequestMessage.Headers.Add("Shuttle.Access", _token);
        }
        finally
        {
            _lock.Release();
        }
    }
}