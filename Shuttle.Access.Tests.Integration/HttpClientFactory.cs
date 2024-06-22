using System.Net.Http;

namespace Shuttle.Access.Tests.Integration;

internal class HttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _httpClient;

    public HttpClientFactory(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public HttpClient CreateClient(string name)
    {
        return _httpClient;
    }
}