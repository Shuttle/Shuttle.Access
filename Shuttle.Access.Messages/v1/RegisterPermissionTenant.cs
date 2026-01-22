namespace Shuttle.Access.Messages.v1;

public class RegisterPermissionTenant : AuditMessage
{
    public string Name { get; set; } = string.Empty;
}