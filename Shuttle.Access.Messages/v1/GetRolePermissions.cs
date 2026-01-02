namespace Shuttle.Access.Messages.v1;

public class GetRolePermissions
{
    public List<string> Permissions { get; set; } = [];
    public Guid RoleId { get; set; }
}