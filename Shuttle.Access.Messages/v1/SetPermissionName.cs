namespace Shuttle.Access.Messages.v1;

public class SetPermissionName : AuditMessage
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}