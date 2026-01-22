namespace Shuttle.Access.Messages.v1;

public class SetTenantStatus : AuditMessage
{
    public Guid Id { get; set; }
    public int Status { get; set; }
}
