namespace Shuttle.Access.Messages.v1;

public class SetPermissionName
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}