namespace Shuttle.Access.WebApi.Contracts.v1;

public class RegisterDelegatedSession
{
    public Guid TenantId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
}