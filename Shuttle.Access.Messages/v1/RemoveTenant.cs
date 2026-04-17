namespace Shuttle.Access.Messages.v1;

public class RemoveTenant : AuditMessage
{
    public Guid Id { get; set; }
}