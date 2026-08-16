using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class ActivateIdentity(Guid? id, string name, Guid auditTenantId, string auditIdentityName)
    : IAuditInformation
{
    public Guid? Id { get; } = id;
    public string Name { get; } = name;
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
}
