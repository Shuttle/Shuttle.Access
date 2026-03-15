namespace Shuttle.Access.WebApi.Contracts.v1;

public class SetIdentityRoleStatus
{
    public bool Active { get; set; }
    public Guid IdentityId { get; set; }
    public Guid RoleId { get; set; }
}