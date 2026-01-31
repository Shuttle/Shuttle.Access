namespace Shuttle.Access;

public class SessionCache : ISessionCache
{
    private readonly List<SessionEntry> _sessionEntries = [];
    private readonly Lock _lock = new();

    private Messages.v1.Session? ActiveSessionOnly(Messages.v1.Session? session)
    {
        if (session == null || DateTimeOffset.UtcNow <= session.ExpiryDate)
        {
            return session;
        }

        Flush(session.IdentityId);

        return null;
    }

    public Messages.v1.Session? Find(Messages.v1.Session.Specification specification)
    {
        lock (_lock)
        {
            var query = _sessionEntries.AsEnumerable();

            if (specification.Token.HasValue)
            {
                query = query.Where(e => e.Token.HasValue && e.Token.Value == specification.Token.Value);
            }

            if (specification.TenantId.HasValue)
            {
                query = query.Where(e => e.Session.TenantId == specification.TenantId.Value);
            }

            if (specification.IdentityId.HasValue)
            {
                query = query.Where(e => e.Session.IdentityId == specification.IdentityId.Value);
            }

            if (!string.IsNullOrWhiteSpace(specification.IdentityName))
            {
                query = query.Where(e => e.Session.IdentityName.Equals(specification.IdentityName, StringComparison.InvariantCultureIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(specification.IdentityNameMatch))
            {
                query = query.Where(e => e.Session.IdentityName.Contains(specification.IdentityNameMatch, StringComparison.InvariantCultureIgnoreCase));
            }

            var sessions = query.Select(e => e.Session).ToList();

            return sessions.Count > 1 
                ? throw new ApplicationException(string.Format(Resources.SessionCountException, sessions.Count)) 
                : ActiveSessionOnly(sessions.FirstOrDefault());
        }
    }

    public Messages.v1.Session Add(Guid? token, Messages.v1.Session session)
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

    public void Flush(Guid identityId)
    {
        lock (_lock)
        {
            _sessionEntries.RemoveAll(item => item.Session.IdentityId.Equals(identityId));
        }
    }

    private class SessionEntry(Guid? token, Messages.v1.Session session)
    {
        public Messages.v1.Session Session { get; } = session;
        public Guid? Token { get; } = token;
    }
}