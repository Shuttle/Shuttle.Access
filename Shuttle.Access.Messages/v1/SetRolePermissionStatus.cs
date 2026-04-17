namespace Shuttle.Access.Messages.v1;

public class SetRolePermissionStatus : AuditMessage
{
    public bool Active { get; set; }
    public Guid PermissionId { get; set; }
    public Guid RoleId { get; set; }
}