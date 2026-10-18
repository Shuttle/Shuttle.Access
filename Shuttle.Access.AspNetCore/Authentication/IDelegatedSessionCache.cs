namespace Shuttle.Access.AspNetCore;

/// <summary>
///     Caches the session resolved by <see cref="DelegatedSessionResolver" />, keyed by the raw `Authorization`
///     header presented by the caller, so that an identical credential does not require a fresh round trip to the
///     Shuttle.Access web API on every request.
/// </summary>
public interface IDelegatedSessionCache
{
    Query.Session? Find(string authorizationHeader);
    Query.Session Add(string authorizationHeader, Query.Session session);
    void Flush();
    void Flush(Guid identityId);
}
