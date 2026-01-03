using System.Net.Http.Headers;

namespace Shuttle.Access.RestClient;

public interface IAuthenticationProvider
{
    Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken = default);
}