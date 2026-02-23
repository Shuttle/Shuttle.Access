namespace Shuttle.Access.Messages.v1;

public class IdentityTenantRemoved
{
    public Guid IdentityId { get; set; }
    public Guid TenantId { get; set; }
}