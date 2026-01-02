namespace Shuttle.Access.Messages.v1;

public class ServerConfiguration
{
    public bool AllowPasswordAuthentication { get; set; }
    public string Version { get; set; } = string.Empty;
}