namespace Shuttle.Access.Messages
{
    public class IdentifierAvailability<T>
    {
        public T Id { get; set; }
        public bool Active { get; set; }
    }
}