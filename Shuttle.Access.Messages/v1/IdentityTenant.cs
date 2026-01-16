namespace Shuttle.Access.Messages.v1;

public class IdentityTenant
{
    public Guid IdentityId { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }
    public string RegisteredBy { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public int Status { get; set; }
}