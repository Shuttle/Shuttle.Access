namespace Shuttle.Access.Messages.v1;

public class SessionDeleted
{
    public Guid Id { get; set; }
    public Guid IdentityId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
}