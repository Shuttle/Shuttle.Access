namespace Shuttle.Access.Messages.v1;

public class RegisterPermission : AuditMessage
{
    public string Description { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public List<Guid> TenantIds { get; set; } = [];
}