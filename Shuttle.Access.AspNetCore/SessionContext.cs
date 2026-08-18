namespace Shuttle.Access.AspNetCore;

public class SessionContext : ISessionContext
{
    public Guid TenantId { get; set; }
    public Query.Session Session { get; set; } = Query.Session.Empty;
    public bool IsAuthorized => !TenantId.Equals(Guid.Empty) && !string.IsNullOrWhiteSpace(Session.IdentityName);
}