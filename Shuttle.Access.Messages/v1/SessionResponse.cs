namespace Shuttle.Access.Messages.v1;

public class SessionResponse
{
    public DateTimeOffset DateRegistered { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public Guid IdentityId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
    public bool RegistrationRequested { get; set; }
    public string Result { get; set; } = string.Empty;
    public string SessionTokenExchangeUrl { get; set; } = string.Empty;
    public Guid Token { get; set; }
    public Guid TenantId { get; set; }
}