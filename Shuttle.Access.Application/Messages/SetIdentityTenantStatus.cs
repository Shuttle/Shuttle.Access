using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class SetIdentityTenantStatus(Guid identityId, Guid tenantId, bool active, Guid auditTenantId, string auditIdentityName)
    : IAuditInformation
{
    public Guid IdentityId { get; } = Guard.AgainstEmpty(identityId);
    public Guid TenantId { get; } = Guard.AgainstEmpty(tenantId);
    public bool Active { get; } = active;
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
}
