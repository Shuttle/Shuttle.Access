using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

/// <summary>
///     In-memory, process-local cache of sessions resolved by <see cref="DelegatedSessionResolver" />.
/// </summary>
/// <remarks>
///     Entries are evicted proactively when <see cref="SessionCacheInvalidationHandler" /> observes that Shuttle.Access
///     has published a session change — see <see cref="HopperBuilderExtensions.AddAccessSessionCacheInvalidation" />.
///     The expiry here is only a safety net for a missed message, not the primary invalidation mechanism.
/// </remarks>
public class DelegatedSessionCache : IDelegatedSessionCache
{
    private readonly Dictionary<string, CacheEntry> _entries = new();
    private readonly Lock _lock = new();

    public Query.Session? Find(string authorizationHeader)
    {
        Guard.AgainstEmpty(authorizationHeader);

        lock (_lock)
        {
            if (!_entries.TryGetValue(authorizationHeader, out var entry))
            {
                return null;
            }

            if (entry.ExpiryDate > DateTimeOffset.UtcNow)
            {
                return entry.Session;
            }

            _entries.Remove(authorizationHeader);

            return null;
        }
    }

    public Query.Session Add(string authorizationHeader, Query.Session session)
    {
        Guard.AgainstEmpty(authorizationHeader);
        Guard.AgainstNull(session);

        lock (_lock)
        {
            _entries[authorizationHeader] = new(session);
        }

        return session;
    }

    public void Flush()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }

    public void Flush(Guid identityId)
    {
        lock (_lock)
        {
            foreach (var key in _entries.Where(item => item.Value.Session.IdentityId.Equals(identityId)).Select(item => item.Key).ToList())
            {
                _entries.Remove(key);
            }
        }
    }

    private class CacheEntry(Query.Session session)
    {
        public DateTimeOffset ExpiryDate { get; } = DateTimeOffset.UtcNow.AddMinutes(5);
        public Query.Session Session { get; } = session;
    }
}
