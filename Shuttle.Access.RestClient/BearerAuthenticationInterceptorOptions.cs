namespace Shuttle.Access.RestClient;

public class BearerAuthenticationInterceptorOptions
{
    public Func<HttpRequestMessage, IServiceProvider, Task<BearerAuthenticationContext>>? GetBearerAuthenticationContextAsync { get; set; }
}