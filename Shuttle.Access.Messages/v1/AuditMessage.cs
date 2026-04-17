namespace Shuttle.Access.Messages.v1;

public class AuditMessage
{
    public string AuditIdentityName { get; set; } = string.Empty;
    public Guid AuditTenantId { get; set; }
}