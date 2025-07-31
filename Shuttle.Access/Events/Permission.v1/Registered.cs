namespace Shuttle.Access.Events.Permission.v1;

public class Registered
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PermissionStatus Status { get; set; }
}