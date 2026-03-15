using Shuttle.Core.Contract;

namespace Shuttle.Access;

public class AuditInformation(Guid tenantId, string identityName) : IAuditInformation
{
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(identityName);
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(tenantId);
}