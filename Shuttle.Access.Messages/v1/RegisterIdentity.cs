namespace Shuttle.Access.Messages.v1;

public class RegisterIdentity : AuditMessage
{
    public Guid? TenantId { get; set; }
    public bool Activated { get; set; }
    public string Description { get; set; } = string.Empty;
    public string GeneratedPassword { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = [];
    public string System { get; set; } = string.Empty;
}