namespace Shuttle.Access.WebApi;

public class SessionContext : ISessionContext
{
    public Session? Session { get; set; }
}