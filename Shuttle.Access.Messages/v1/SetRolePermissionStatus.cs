namespace Shuttle.Access.Messages.v1;

public class SetRolePermissionStatus
{
    public bool Active { get; set; }
    public Guid PermissionId { get; set; }
    public Guid RoleId { get; set; }
    public string AuditIdentityName { get; set; } = string.Empty;
}