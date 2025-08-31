using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class SessionAvailableEventArgs(Messages.v1.Session session)
{
    public Messages.v1.Session Session { get; } = Guard.AgainstNull(session);
}