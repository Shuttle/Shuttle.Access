namespace Shuttle.Access.Messages.v1;

public class SetIdentityTenantStatus : AuditMessage
{
    public bool Active { get; set; }
    public Guid IdentityId { get; set; }
    public Guid TenantId { get; set; }
}