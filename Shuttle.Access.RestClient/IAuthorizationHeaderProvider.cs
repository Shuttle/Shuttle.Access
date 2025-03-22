using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.RestClient;

public interface IAuthenticationProvider
{
    string Name { get; }
    Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(CancellationToken cancellationToken = default);
}