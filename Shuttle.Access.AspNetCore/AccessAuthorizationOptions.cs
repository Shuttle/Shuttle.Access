using Shuttle.Extensions.Options;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationOptions
{
    public const string SectionName = "Shuttle:Access:Authorization";
    
    public List<IssuerOptions> Issuers { get; set; } = [];
    public bool PassThrough { get; set; } = true;

    public AsyncEvent<AuthorizationHeaderAvailableEventArgs> AuthorizationHeaderAvailable { get; set; } = new(); 
    public AsyncEvent<SessionAvailableEventArgs> SessionAvailable { get; set; } = new(); 
    public AsyncEvent<SessionUnavailableEventArgs> SessionUnavailable { get; set; } = new(); 
}