using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Shuttle.Access.WebApi.Contracts.v1;
using Shuttle.Contract;

namespace Shuttle.Access.RestClient;

public class PasswordAuthenticationInterceptor(IOptions<AccessClientOptions> accessClientOptions, IOptions<PasswordAuthenticationInterceptorOptions> passwordAuthenticationInterceptorOptions, HttpClient httpClient)
    : IAuthenticationInterceptor
{
    private readonly AccessClientOptions _accessClientOptions = Guard.AgainstNull(Guard.AgainstNull(accessClientOptions).Value);
    private readonly string _baseAddress = accessClientOptions.Value.BaseAddress.TrimEnd('/');
    private readonly HttpClient _httpClient = Guard.AgainstNull(httpClient);
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly PasswordAuthenticationInterceptorOptions _passwordAuthenticationInterceptorOptions = Guard.AgainstNull(Guard.AgainstNull(passwordAuthenticationInterceptorOptions).Value);
    private DateTimeOffset _tokenExpiryDate = DateTimeOffset.MinValue;
    private string _token = string.Empty;

    public async Task ConfigureAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(httpRequestMessage);

        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_passwordAuthenticationInterceptorOptions.TenantId.HasValue)
            {
                httpRequestMessage.Headers.Add(AccessClient.TenantIdHeaderName, $"{_passwordAuthenticationInterceptorOptions.TenantId.Value:D}");
            }

            if (_tokenExpiryDate > DateTimeOffset.UtcNow.Add(_accessClientOptions.RenewToleranceTimeSpan))
            {
                httpRequestMessage.Headers.Authorization = new("Shuttle.Access", _token);
                return;
            }

            var requestData = new
            {
                identityName = _passwordAuthenticationInterceptorOptions.IdentityName,
                password = _passwordAuthenticationInterceptorOptions.Password
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseAddress}/v1/sessions", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new AuthenticationException(response.ReasonPhrase);
            }

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            var sessionResponse = JsonSerializer.Deserialize<SessionResponse>(responseString, _jsonSerializerOptions);

            if (sessionResponse?.Session == null || !sessionResponse.IsSuccessResult())
            {
                throw new AuthenticationException(sessionResponse == null ? null : string.Format(Resources.SessionResponseException, sessionResponse.Result));
            }

            _token = $"token={sessionResponse.Token:D}";
            _tokenExpiryDate = sessionResponse.Session.ExpiryDate;

            httpRequestMessage.Headers.Authorization = new("Shuttle.Access", _token);
        }
        finally
        {
            _lock.Release();
        }
    }
}
