using Shuttle.Extensions.Options;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationOptions
{
    public const string SectionName = "Shuttle:Access:Authorization";

    public AsyncEvent<AuthorizationHeaderAvailableEventArgs> AuthorizationHeaderAvailable { get; set; } = new();

    public bool InsecureModeEnabled { get; set; }
    public List<IssuerOptions> Issuers { get; set; } = [];
    public AsyncEvent<JwtIssuerOptionsAvailableEventArgs> JwtIssuerOptionsAvailable { get; set; } = new();
    public AsyncEvent<JwtIssuerOptionsUnavailableEventArgs> JwtIssuerOptionsUnavailable { get; set; } = new();
    public bool PassThrough { get; set; } = true;
    public AsyncEvent<SessionAvailableEventArgs> SessionAvailable { get; set; } = new();
    public AsyncEvent<SessionUnavailableEventArgs> SessionUnavailable { get; set; } = new();
}