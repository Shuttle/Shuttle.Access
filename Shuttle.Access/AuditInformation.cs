using Shuttle.Core.Contract;

namespace Shuttle.Access;

public class AuditInformation(Guid tenantId, string identityName) : IAuditInformation
{
    public string IdentityName { get; } = Guard.AgainstEmpty(identityName);
    public Guid TenantId { get; } = Guard.AgainstEmpty(tenantId);
}