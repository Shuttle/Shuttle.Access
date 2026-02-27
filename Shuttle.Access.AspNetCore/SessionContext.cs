using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class SessionContext : ISessionContext
{
    public Shuttle.Access.Messages.v1.Session? Session { get; set; }

    public bool IsAuthorized => Session is { TenantId: not null } && !string.IsNullOrWhiteSpace(Session?.IdentityName);
}