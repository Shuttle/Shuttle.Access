using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class SetRolePermissionStatus(Guid roleId, Guid permissionId, bool active, Guid auditTenantId, string auditIdentityName)
    : IAuditInformation
{
    public Guid RoleId { get; } = Guard.AgainstEmpty(roleId);
    public Guid PermissionId { get; } = Guard.AgainstEmpty(permissionId);
    public bool Active { get; } = active;
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
}
