using Shuttle.Extensions.Options;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationOptions
{
    public const string SectionName = "Shuttle:Access:Authorization";

    public AsyncEvent<AuthorizationHeaderAvailableEventArgs> AuthorizationHeaderAvailable { get; set; } = new();

    /// <summary>
    ///     The address of the Shuttle.Access web API, which resolves the caller's session.  Required by the default
    ///     <see cref="DelegatedSessionResolver" />; the Shuttle.Access web API itself resolves sessions directly and
    ///     therefore leaves this empty.
    /// </summary>
    public string BaseAddress { get; set; } = string.Empty;

    public string Realm { get; set; } = "API";
    public AsyncEvent<SessionAvailableEventArgs> SessionAvailable { get; set; } = new();
    public AsyncEvent<SessionUnavailableEventArgs> SessionUnavailable { get; set; } = new();
}
