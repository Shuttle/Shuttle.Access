namespace Shuttle.Access.Messages.v1;

public class RegisterRole
{
    public string Name { get; set; } = string.Empty;
    public List<RegisterPermission> Permissions { get; set; } = [];
    public int WaitCount { get; set; }
    public string AuditIdentityName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}