namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationOptions
{
    public const string SectionName = "Shuttle:Access.Authorization";
    public List<IssuerOptions> Issuers { get; set; } = [];
}

public class IssuerOptions
{
    public string JwksUri { get; set; } = string.Empty; // JSON Web Key Store
    public string Uri { get; set; } = string.Empty;
    public List<string> Audiences { get; set; } = [];
    public List<string> IdentityNameClaimTypes { get; set; } = [];
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan SigningKeyCacheDuration { get; set; } = TimeSpan.FromHours(1);
}
