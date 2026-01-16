namespace Shuttle.Access.WebApi;

public interface ISessionContext
{
    public Session? Session { get; set; }
}