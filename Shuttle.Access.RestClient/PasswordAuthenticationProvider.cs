using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class PasswordAuthenticationProvider : IAuthenticationProvider
{
    private readonly AccessClientOptions _accessClientOptions;
    private readonly string _baseAddress;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly PasswordAuthenticationProviderOptions _passwordAuthenticationProviderOptions;
    private DateTimeOffset _expiryDate = DateTimeOffset.MinValue;
    private string _token = string.Empty;

    public PasswordAuthenticationProvider(IOptions<AccessClientOptions> accessClientOptions, IOptions<PasswordAuthenticationProviderOptions> passwordAuthenticationProviderOptions, HttpClient httpClient)
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

    public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_expiryDate > DateTimeOffset.UtcNow.Add(_accessClientOptions.RenewToleranceTimeSpan))
            {
                return new("Shuttle.Access", _token);
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