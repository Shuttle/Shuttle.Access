using System.Threading.Tasks;
using System;
using System.Net.Http;

namespace Shuttle.Access.RestClient;

public class BearerAuthenticationProviderOptions
{
    public Func<HttpRequestMessage, IServiceProvider, ValueTask<string>>? GetTokenAsync { get; set; } = null!;
}