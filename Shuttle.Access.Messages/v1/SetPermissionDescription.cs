namespace Shuttle.Access.Messages.v1;

public class SetPermissionDescription : AuditMessage
{
    public string Description { get; set; } = string.Empty;
    public Guid Id { get; set; }
}