using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class SetTenantMaximumIdentities(Guid id, int maximumIdentities, Guid auditTenantId, string auditIdentityName)
    : IAuditInformation
{
    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public int MaximumIdentities { get; } = maximumIdentities;
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
}
