namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationOptions
{
    public const string SectionName = "Shuttle:Access:Authorization";
    public List<IssuerOptions> Issuers { get; set; } = [];
}