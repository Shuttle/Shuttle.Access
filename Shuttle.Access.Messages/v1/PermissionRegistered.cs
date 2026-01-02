namespace Shuttle.Access.Messages.v1;

public class PermissionRegistered
{
    public string Description { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}