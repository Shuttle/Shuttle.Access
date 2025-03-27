namespace Shuttle.Access.AspNetCore;

public class IssuerOptions
{
    public List<string> Audiences { get; set; } = [];
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);
    public List<string> IdentityNameClaimTypes { get; set; } = [];
    public string JwksUri { get; set; } = string.Empty; // JSON Web Key Store
    public TimeSpan SigningKeyCacheDuration { get; set; } = TimeSpan.FromHours(1);
    public string Uri { get; set; } = string.Empty;
}