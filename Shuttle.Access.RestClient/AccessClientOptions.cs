using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shuttle.Access.RestClient;

public class AccessClientOptions
{
    public const string SectionName = "Shuttle:Access:Client";

    public string BaseAddress { get; set; } = string.Empty;
    public TimeSpan RenewToleranceTimeSpan { get; set; } = TimeSpan.FromMinutes(5);
    public Func<HttpRequestMessage, IServiceProvider, Task>? ConfigureHttpRequestAsync { get; set; }
}