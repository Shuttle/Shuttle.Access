namespace Shuttle.Access;

public interface IAuditInformation
{
    public string IdentityName { get; }
    public Guid TenantId { get; }
}