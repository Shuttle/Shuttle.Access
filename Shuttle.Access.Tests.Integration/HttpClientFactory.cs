namespace Shuttle.Access.Tests.Integration;

internal class HttpClientFactory(HttpClient httpClient) : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return httpClient;
    }
}