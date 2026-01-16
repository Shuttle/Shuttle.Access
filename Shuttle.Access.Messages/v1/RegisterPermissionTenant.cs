namespace Shuttle.Access.Messages.v1;

public class RegisterPermissionTenant
{
    public string Name { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string AuditIdentityName { get; set; } = string.Empty;
}