namespace Shuttle.Access.Messages.v1;

public class RegisterSession
{
    public Guid? TenantId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid Token { get; set; }
}