namespace Shuttle.Access.Messages.v1;

public class RolePermissionAdded
{
    public Guid PermissionId { get; set; }
    public Guid RoleId { get; set; }
}