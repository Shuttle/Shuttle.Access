using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class PasswordAuthenticationInterceptor : IAuthenticationInterceptor
{
    private readonly AccessClientOptions _accessClientOptions;
    private readonly string _baseAddress;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly PasswordAuthenticationProviderOptions _passwordAuthenticationProviderOptions;
    private DateTimeOffset _tokenExpiryDate = DateTimeOffset.MinValue;
    private string _token = string.Empty;

    public PasswordAuthenticationInterceptor(IOptions<AccessClientOptions> accessClientOptions, IOptions<PasswordAuthenticationProviderOptions> passwordAuthenticationProviderOptions, HttpClient httpClient)
    {
        _accessClientOptions = Guard.AgainstNull(Guard.AgainstNull(accessClientOptions).Value);
        _passwordAuthenticationProviderOptions = Guard.AgainstNull(Guard.AgainstNull(passwordAuthenticationProviderOptions).Value);
        _httpClient = Guard.AgainstNull(httpClient);

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

            var requestData = new
            {
                identityName = _passwordAuthenticationProviderOptions.IdentityName,
                password = _passwordAuthenticationProviderOptions.Password
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

            if (sessionResponse == null || !sessionResponse.IsSuccessResult())
            {
                throw new AuthenticationException(sessionResponse == null ? null : string.Format(Resources.SessionResponseException, sessionResponse.Result));
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