namespace Shuttle.Access.Application;

public class ConfigureApplication(TimeSpan timeout)
{
    public TimeSpan Timeout { get; } = timeout;
    public bool ShouldRetry { get; private set; }

    public ConfigureApplication Retry()
    {
        ShouldRetry = true;
        return this;
    }
}
