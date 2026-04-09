using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

public class SessionAvailableEventArgs(Query.Session session)
{
    public Query.Session Session { get; } = Guard.AgainstNull(session);
}