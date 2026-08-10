using Shuttle.Extensions.Options;

namespace Shuttle.Access.WebApi;

/// <summary>
///     Credential validation options for the Shuttle.Access web API.  These live here, and not in
///     `Shuttle.Access.AspNetCore`, because the web API is the only application that validates issuers and session
///     tokens — every other application asks the web API who the caller is.
/// </summary>
public class AccessAuthenticationOptions
{
    public const string SectionName = "Shuttle:Access:Authorization";

    public bool InsecureModeEnabled { get; set; }
    public List<IssuerOptions> Issuers { get; set; } = [];
    public AsyncEvent<JwtIssuerOptionsAvailableEventArgs> JwtIssuerOptionsAvailable { get; set; } = new();
    public AsyncEvent<JwtIssuerOptionsUnavailableEventArgs> JwtIssuerOptionsUnavailable { get; set; } = new();
}
