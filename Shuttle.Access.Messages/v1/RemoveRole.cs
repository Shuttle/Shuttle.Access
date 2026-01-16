namespace Shuttle.Access.Messages.v1;

public class RemoveRole
{
    public Guid Id { get; set; }
    public string AuditIdentityName { get; set; } = string.Empty;
}