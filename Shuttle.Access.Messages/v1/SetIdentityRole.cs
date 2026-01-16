namespace Shuttle.Access.Messages.v1;

public class SetIdentityRoleStatus
{
    public bool Active { get; set; }
    public Guid IdentityId { get; set; }
    public Guid RoleId { get; set; }
    public string AuditIdentityName { get; set; } = string.Empty;
}