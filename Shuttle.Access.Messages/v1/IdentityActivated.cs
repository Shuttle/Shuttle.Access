namespace Shuttle.Access.Messages.v1;

public class IdentityActivated
{
    public DateTimeOffset DateActivated { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}