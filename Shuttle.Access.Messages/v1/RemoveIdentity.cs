namespace Shuttle.Access.Messages.v1;

public class RemoveIdentity : AuditMessage
{
    public Guid Id { get; set; }
}