using Shuttle.Access.Messages.v1;

namespace Shuttle.Access;

public abstract class SessionCache
{
    private readonly List<SessionEntry> _sessionEntries = [];
    private readonly object _lock = new();

    private Messages.v1.Session? ActiveSessionOnly(Messages.v1.Session? session)
    {
        if (session == null || DateTimeOffset.UtcNow <= session.ExpiryDate)
        {
            return session;
        }

        Flush(session.IdentityId);

        return null;
    }

    protected Messages.v1.Session? FindByToken(Guid token)
    {
        lock (_lock)
        {
            return ActiveSessionOnly(_sessionEntries.FirstOrDefault(item => item.Token.HasValue && item.Token.Equals(token))?.Session);
        }
    }

    protected Messages.v1.Session? Find(Guid identityId)
    {
        lock (_lock)
        {
            return ActiveSessionOnly(_sessionEntries.FirstOrDefault(item => item.Session.IdentityId.Equals(identityId))?.Session);
        }
    }

    protected Messages.v1.Session? Find(string identityName)
    {
        lock (_lock)
        {
            return ActiveSessionOnly(_sessionEntries.FirstOrDefault(item => item.Session.IdentityName.Equals(identityName, StringComparison.InvariantCultureIgnoreCase))?.Session);
        }
    }

    protected Messages.v1.Session Add(Guid? token, Messages.v1.Session session)
    {
        lock (_lock)
        {
            _sessionEntries.RemoveAll(item => item.Session.IdentityId.Equals(session.IdentityId));
            _sessionEntries.Add(new(token, session));

            return session;
        }
    }

    public void Flush()
    {
        lock (_lock)
        {
            _sessionEntries.Clear();
        }
    }

    protected void Flush(Guid identityId)
    {
        lock (_lock)
        {
            _sessionEntries.RemoveAll(item => item.Session.IdentityId.Equals(identityId));
        }
    }

    protected bool HasPermission(Guid identityId, string requiredPermission)
    {
        lock (_lock)
        {
            var sessionEntry = _sessionEntries.FirstOrDefault(item => item.Session.IdentityId.Equals(identityId));

            return sessionEntry != null && sessionEntry.Session.HasPermission(requiredPermission);
        }
    }

    private class SessionEntry(Guid? token, Messages.v1.Session session)
    {
        public Messages.v1.Session Session { get; } = session;
        public Guid? Token { get; } = token;
    }
}