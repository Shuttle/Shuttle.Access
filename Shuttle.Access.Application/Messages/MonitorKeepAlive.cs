namespace Shuttle.Access.Application;

public class MonitorKeepAlive
{
    public bool ShouldReset { get; private set; }

    public MonitorKeepAlive Reset()
    {
        ShouldReset = true;
        return this;
    }
}
