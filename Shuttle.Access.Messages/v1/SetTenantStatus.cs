namespace Shuttle.Access.Messages.v1;

public class SetTenantStatus
{
    public Guid Id { get; set; }
    public int Status { get; set; }
    public string AuditIdentityName { get; set; } = string.Empty;
}
