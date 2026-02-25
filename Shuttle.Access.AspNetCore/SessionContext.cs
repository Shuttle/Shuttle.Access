using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

internal class SessionContext : ISessionContext
{
    public Shuttle.Access.Messages.v1.Session? Session { get; private set; }

    internal void WithSession(Shuttle.Access.Messages.v1.Session? session)
    {
        Session = Guard.AgainstNull(session);
    }
}