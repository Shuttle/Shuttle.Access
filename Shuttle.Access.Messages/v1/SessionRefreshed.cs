namespace Shuttle.Access.Messages.v1;

public class SessionRefreshed
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid IdentityId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
}