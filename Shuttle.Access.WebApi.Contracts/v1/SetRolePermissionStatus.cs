namespace Shuttle.Access.WebApi.Contracts.v1;

public class SetRolePermissionStatus
{
    public bool Active { get; set; }
    public Guid PermissionId { get; set; }
}