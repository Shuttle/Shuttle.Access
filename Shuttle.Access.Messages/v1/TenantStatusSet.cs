namespace Shuttle.Access.Messages.v1;

public class TenantStatusSet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public int Version { get; set; }
}