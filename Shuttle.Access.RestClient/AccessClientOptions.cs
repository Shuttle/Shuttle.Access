using System;

namespace Shuttle.Access.RestClient;

public class AccessClientOptions
{
    public const string SectionName = "Shuttle:Access:Client";

    public string BaseAddress { get; set; } = string.Empty;
    public string IdentityName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public TimeSpan RenewToleranceTimeSpan { get; set; } = TimeSpan.FromMinutes(5);
}