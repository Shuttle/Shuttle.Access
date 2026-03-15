using Shuttle.Core.Contract;

namespace Shuttle.Access.Application;

public class TenantSelected(Guid sessionId, Guid tenantId)
{
    public Guid SessionId { get; } = Guard.AgainstEmpty(sessionId);
    public Guid TenantId { get; } = Guard.AgainstEmpty(tenantId);
    public Query.Session? Session { get; private set; }

    public TenantSelected WithSession(Query.Session session)
    {
        Session = Guard.AgainstNull(session);
        return this;
    }
}