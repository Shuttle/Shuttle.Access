namespace Shuttle.Access.Events.AttributeDefinition.v1;

public class Registered
{
    public AttributeCardinality Cardinality { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public AttributeType Type { get; set; }
}
