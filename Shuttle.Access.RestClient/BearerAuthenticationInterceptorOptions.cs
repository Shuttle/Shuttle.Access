namespace Shuttle.Access.RestClient;

public class BearerAuthenticationInterceptorOptions
{
    public Func<HttpRequestMessage, IServiceProvider, Task<BearerAuthenticationContext>>? GetBearerAuthenticationContextAsync { get; set; }
    public Guid? TenantId { get; set; }
    public string Application { get; set; } = "Access";
}