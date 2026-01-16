namespace Shuttle.Access.Messages.v1;

public class SetPermissionDescription
{
    public string Description { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string AuditIdentityName { get; set; } = string.Empty;
}