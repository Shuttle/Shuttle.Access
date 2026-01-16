namespace Shuttle.Access.Events.Role.v2;

public class Registered
{
    public string Name { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}