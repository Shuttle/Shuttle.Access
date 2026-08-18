using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class SetIdentityRoleStatus(Guid identityId, Guid roleId, bool active, Guid auditTenantId, string auditIdentityName)
    : IAuditInformation
{
    public Guid IdentityId { get; } = Guard.AgainstEmpty(identityId);
    public Guid RoleId { get; } = Guard.AgainstEmpty(roleId);
    public bool Active { get; } = active;
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
}
