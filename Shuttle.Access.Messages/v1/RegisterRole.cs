namespace Shuttle.Access.Messages.v1;

public class RegisterRole : AuditMessage
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<RegisterPermission> Permissions { get; set; } = [];
    public int WaitCount { get; set; }
}