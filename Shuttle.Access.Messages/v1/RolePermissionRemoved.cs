namespace Shuttle.Access.Messages.v1;

public class RolePermissionRemoved
{
    public Guid PermissionId { get; set; }
    public Guid RoleId { get; set; }
}