namespace Shuttle.Access.Events.Identity.v1;

public class AttributeValueAdded
{
    public Guid AttributeDefinitionId { get; set; }
    public string Value { get; set; } = string.Empty;
}
