namespace Shuttle.Access.Messages.v1;

public class RemoveIdentity
{
    public Guid Id { get; set; }
    public string AuditIdentityName { get; set; } = string.Empty;
}