namespace Shuttle.Access;

public class SessionCache(IHashingService hashingService) : ISessionCache
{
    private readonly Lock _lock = new();
    private readonly List<SessionEntry> _sessionEntries = [];

    private Query.Session? ActiveSessionOnly(Query.Session? session)
    {
        if (session == null || DateTimeOffset.UtcNow <= session.ExpiryDate)
        {
            return session;
        }

        Flush(session.IdentityId);

        return null;
    }

    public Query.Session? Find(Query.Session.Specification specification)
    {
        lock (_lock)
        {
            var query = _sessionEntries.AsEnumerable();

            if (specification.Token.HasValue)
            {
                specification.WithTokenHash(hashingService.Sha256($"{specification.Token.Value:D}"));
            }

            if (specification.TokenHash != null)
            {
                query = query.Where(e => e.Session.TokenHash.SequenceEqual(specification.TokenHash));
            }

            if (specification.TenantId.HasValue)
            {
                query = query.Where(e => e.Session.TenantId == specification.TenantId);
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

            var sessions = query
                .Where(e => e.ExpiryDate > DateTimeOffset.UtcNow)
                .Select(e => e.Session).ToList();

            return sessions.Count > 1
                ? throw new ApplicationException(string.Format(Resources.SessionCountException, sessions.Count))
                : ActiveSessionOnly(sessions.FirstOrDefault());
        }
    }

    public Query.Session Add(Query.Session session)
    {
        lock (_lock)
        {
            _sessionEntries.RemoveAll(item => item.Session.TenantId == session.TenantId && item.Session.IdentityId.Equals(session.IdentityId));
            _sessionEntries.Add(new(session));

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

    private class SessionEntry(Query.Session session)
    {
        public Query.Session Session { get; } = session;
        public DateTimeOffset ExpiryDate { get; } = DateTimeOffset.UtcNow.AddMinutes(5);
    }
}