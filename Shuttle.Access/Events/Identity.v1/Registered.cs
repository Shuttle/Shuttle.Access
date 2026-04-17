namespace Shuttle.Access.Events.Identity.v1;

public class Registered
{
    public bool Activated { get; set; }
    public DateTimeOffset DateRegistered { get; set; }
    public string Description { get; set; } = string.Empty;
    public string GeneratedPassword { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = [];
    public string RegisteredBy { get; set; } = string.Empty;
}