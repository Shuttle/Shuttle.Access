namespace Shuttle.Access.Messages.v1;

public class IdentityRoleAdded
{
    public Guid IdentityId { get; set; }
    public Guid RoleId { get; set; }
}