namespace Shuttle.Access.Messages.v1;

public class SetTenantMaximumIdentities : AuditMessage
{
    public Guid Id { get; set; }
    public int MaximumIdentities { get; set; }
}
