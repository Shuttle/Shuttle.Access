namespace Shuttle.Access.Messages.v1;

public class IdentityTenantAdded
{
    public Guid IdentityId { get; set; }
    public Guid TenantId { get; set; }
}