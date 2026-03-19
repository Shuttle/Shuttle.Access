namespace Shuttle.Access.AspNetCore;

public class SessionContext : ISessionContext
{
    public Query.Session? Session { get; set; }

    public bool IsAuthorized => Session != null && !string.IsNullOrWhiteSpace(Session?.IdentityName);
}