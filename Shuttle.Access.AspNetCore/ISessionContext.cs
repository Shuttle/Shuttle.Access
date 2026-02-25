namespace Shuttle.Access.AspNetCore;

public interface ISessionContext
{
    public Shuttle.Access.Messages.v1.Session? Session { get; }
}
