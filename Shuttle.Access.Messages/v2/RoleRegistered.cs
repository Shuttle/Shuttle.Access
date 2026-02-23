namespace Shuttle.Access.Messages.v2;

public class RoleRegistered
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}