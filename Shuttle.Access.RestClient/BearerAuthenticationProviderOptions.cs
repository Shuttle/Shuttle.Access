namespace Shuttle.Access.RestClient;

public class BearerAuthenticationProviderOptions
{
    public Func<HttpRequestMessage, IServiceProvider, Task<BearerAuthenticationContext>>? GetBearerAuthenticationContextAsync { get; set; }
}