using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.RestClient;

public class BearerAuthenticationProvider : IAuthenticationProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AccessClientOptions _accessClientOptions;
    private readonly string _baseAddress;
    private readonly BearerAuthenticationProviderOptions _bearerAuthenticationProviderOptions;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IJwtService _jwtService;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IServiceProvider _serviceProvider;
    private DateTimeOffset _expiryDate = DateTimeOffset.MinValue;
    private string _token = string.Empty;
    private readonly AccessAuthorizationOptions _accessAuthorizationOptions;

    public BearerAuthenticationProvider(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, IOptions<AccessClientOptions> accessClientOptions, IOptions<BearerAuthenticationProviderOptions> bearerAuthenticationProviderOptions, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IJwtService jwtService, IServiceProvider serviceProvider)
    {
        _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
        _accessClientOptions = Guard.AgainstNull(Guard.AgainstNull(accessClientOptions).Value);
        _bearerAuthenticationProviderOptions = Guard.AgainstNull(Guard.AgainstNull(bearerAuthenticationProviderOptions).Value);
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

    public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_expiryDate > DateTimeOffset.UtcNow.Add(_accessClientOptions.RenewToleranceTimeSpan))
            {
                return new("Shuttle.Access", _token);
            }

            var token = await (_bearerAuthenticationProviderOptions.GetTokenAsync?.Invoke(httpRequestMessage, _serviceProvider) ?? ValueTask.FromResult(string.Empty));

            if (_accessAuthorizationOptions.PassThrough)
            {
                if (!string.IsNullOrEmpty(token))
                {
                    return new("Bearer", token);
                }

                token = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();

                if (token == null || string.IsNullOrWhiteSpace(token) || !token.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase) || token.Length < 8)
                {
                    throw new AuthenticationException(Resources.AuthorizationHeaderNotFound);
                }

                return new("Bearer", token[7..]);
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new AuthenticationException(Resources.AuthorizationHeaderNotFound);
            }

            var identityName = await _jwtService.GetIdentityNameAsync(token);

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
            request.Headers.Authorization = new("Bearer", token);

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
            _expiryDate = sessionResponse.ExpiryDate;

            return new("Shuttle.Access", _token);
        }
        finally
        {
            _lock.Release();
        }
    }
}