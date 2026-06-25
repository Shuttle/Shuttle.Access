using Shuttle.Access.Query;

namespace Shuttle.Access;

public class SessionCache(IHashingService hashingService) : ISessionCache
{
    private readonly Lock _lock = new();
    private readonly List<SessionEntry> _sessionEntries = [];

    public Session? Find(Session.Specification specification)
    {
        lock (_lock)
        {
            var query = _sessionEntries.AsEnumerable();

            if (specification.HasIds)
            {
                query = query.Where(e => specification.Ids.Contains(e.Session.Id));
            }

            if (specification.Token.HasValue)
            {
                specification.WithTokenHash(hashingService.Sha256($"{specification.Token.Value:D}"));
            }

            if (!string.IsNullOrWhiteSpace(specification.TokenHash))
            {
                query = query.Where(e => e.Session.Tokens.Any(t => t.TokenHash.Equals(specification.TokenHash, StringComparison.InvariantCultureIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(specification.Application))
            {
                query = query.Where(e => e.Session.Tokens.Any(t => t.Application.Equals(specification.Application, StringComparison.InvariantCultureIgnoreCase)));
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

    public Session Add(Session session)
    {
        lock (_lock)
        {
            _sessionEntries.RemoveAll(item => item.Session.IdentityId.Equals(session.IdentityId));
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

    private Session? ActiveSessionOnly(Session? session)
    {
        if (session == null || DateTimeOffset.UtcNow <= session.ExpiryDate)
        {
            return session;
        }

        Flush(session.IdentityId);

        return null;
    }

    private class SessionEntry(Session session)
    {
        public DateTimeOffset ExpiryDate { get; } = DateTimeOffset.UtcNow.AddMinutes(5);
        public Session Session { get; } = session;
    }
}