namespace Shuttle.Access.Messages.v1;

public class SetIdentityRoleStatus : AuditMessage
{
    public bool Active { get; set; }
    public Guid IdentityId { get; set; }
    public Guid RoleId { get; set; }
}