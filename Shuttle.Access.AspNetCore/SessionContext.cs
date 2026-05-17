namespace Shuttle.Access.AspNetCore;

public class SessionContext : ISessionContext
{
    public Guid TenantId { get; set; }
    public Query.Session? Session { get; set; }
    public bool IsAuthorized => !TenantId.Equals(Guid.Empty) && Session != null && !string.IsNullOrWhiteSpace(Session?.IdentityName);
}