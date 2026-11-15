using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class PromoteIdentityAlternateName(Guid id, string name, IdentityAlternateNamePromotionMode mode, Guid auditTenantId, string auditIdentityName)
    : IAuditInformation
{
    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public string Name { get; } = Guard.AgainstEmpty(name);
    public IdentityAlternateNamePromotionMode Mode { get; } = mode;
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
}
