using Shuttle.Core.Contract;

namespace Shuttle.Access;

public class SessionService(ISessionCache sessionCache, ISessionQuery sessionQuery)
    : ISessionService
{
    private readonly ISessionCache _sessionCache = Guard.AgainstNull(sessionCache);
    private readonly ISessionQuery _sessionQuery = Guard.AgainstNull(sessionQuery);

    public async Task<Query.Session?> FindAsync(Query.Session.Specification specification, CancellationToken cancellationToken = default)
    {
        var session = _sessionCache.Find(specification);

        return session ?? Add((await _sessionQuery.SearchAsync(specification, cancellationToken)).FirstOrDefault());
    }

    private Query.Session? Add(Query.Session? session)
    {
        return session == null ? null : sessionCache.Add(session);
    }
}