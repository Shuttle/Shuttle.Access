namespace Shuttle.Access.WebApi.Contracts.v1;

public class RegisterRole
{
    public Guid? Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<RegisterPermission> Permissions { get; set; } = [];
}