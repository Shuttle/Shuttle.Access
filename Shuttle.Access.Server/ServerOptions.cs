namespace Shuttle.Access.Server;

public class ServerOptions
{
    public const string SectionName = "Shuttle:Access:Server";

    public string AdministratorIdentityName { get; set; } = "shuttle-admin";
    public string AdministratorPassword { get; set; } = "shuttle-admin";
    public TimeSpan MonitorKeepAliveInterval { get; set; } = TimeSpan.FromSeconds(15);
    public bool ShouldConfigure { get; set; } = true;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);
}