namespace Shuttle.Access;

public interface IAuditInformation
{
    public string AuditIdentityName { get; }
    public Guid AuditTenantId { get; }
}