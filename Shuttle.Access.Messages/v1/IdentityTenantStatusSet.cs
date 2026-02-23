namespace Shuttle.Access.Messages.v1;

public class IdentityTenantStatusSet
{
    public bool Active { get; set; }
    public Guid IdentityId { get; set; }
    public Guid TenantId { get; set; }
}