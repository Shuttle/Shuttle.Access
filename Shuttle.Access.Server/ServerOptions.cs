namespace Shuttle.Access.Server;

public class ServerOptions
{
    public const string SectionName = "Shuttle:Access:Server";

    public TimeSpan MonitorKeepAliveInterval { get; set; } = TimeSpan.FromSeconds(15);
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);
}