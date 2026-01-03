namespace Shuttle.Access.RestClient;

public class AccessClientOptions
{
    public const string SectionName = "Shuttle:Access:Client";

    public string BaseAddress { get; set; } = string.Empty;
    public Func<HttpRequestMessage, IServiceProvider, Task>? ConfigureHttpRequestAsync { get; set; }
    public TimeSpan RenewToleranceTimeSpan { get; set; } = TimeSpan.FromMinutes(5);
}