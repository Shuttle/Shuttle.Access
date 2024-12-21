namespace Shuttle.Access.Messages;

public class IdentifierAvailability<T>
{
    public bool Active { get; set; }
    public T Id { get; set; } = default!;
}