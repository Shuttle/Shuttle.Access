namespace Shuttle.Access.Server;

public interface IKeepAliveContext
{
    Task SentAsync();
    ValueTask<bool> GetShouldIgnoreAsync();
    DateTimeOffset GetIgnoreTillDate();
    Task ResetAsync();
}