namespace Shuttle.Access.Messages.v1;

public class RegisterDelegatedSession
{
    public Guid TenantId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
}