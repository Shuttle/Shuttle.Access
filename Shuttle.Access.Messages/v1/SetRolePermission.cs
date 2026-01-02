namespace Shuttle.Access.Messages.v1;

public class SetRolePermission
{
    public bool Active { get; set; }
    public Guid PermissionId { get; set; }
    public Guid RoleId { get; set; }
}