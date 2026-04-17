namespace Shuttle.Access.WebApi.Contracts.v1;

public class IdentifierAvailability<T>
{
    public bool Active { get; set; }
    public T Id { get; set; } = default!;
}