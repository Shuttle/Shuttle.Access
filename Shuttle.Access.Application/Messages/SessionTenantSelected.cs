using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class SessionTenantSelected(Guid sessionId, Guid tenantId)
{
    public Guid SessionId { get; } = Guard.AgainstEmpty(sessionId);
    public Guid TenantId { get; } = Guard.AgainstEmpty(tenantId);
    public Query.Session? Session { get; private set; }

    public SessionTenantSelected WithSession(Query.Session session)
    {
        Session = Guard.AgainstNull(session);
        return this;
    }
}