namespace Shuttle.Access.Messages.v1;

public class IdentityRoleRemoved
{
    public Guid IdentityId { get; set; }
    public Guid RoleId { get; set; }
}