namespace Shuttle.Access.Messages.v1;

public class IdentityRoleSet
{
    public bool Active { get; set; }
    public Guid IdentityId { get; set; }
    public Guid RoleId { get; set; }
    public int Version { get; set; }
}