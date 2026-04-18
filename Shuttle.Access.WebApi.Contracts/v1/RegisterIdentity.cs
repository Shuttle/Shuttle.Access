namespace Shuttle.Access.WebApi.Contracts.v1;

public class RegisterIdentity
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool Activated { get; set; }
    public List<Guid> RoleIds { get; set; } = [];
    public List<Guid> TenantIds { get; set; } = [];
}