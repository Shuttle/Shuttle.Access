namespace Shuttle.Access.Messages.v1;

public class PasswordSet
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
}