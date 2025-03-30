using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.RestClient;

public interface IAuthenticationProvider
{
    Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken = default);
}