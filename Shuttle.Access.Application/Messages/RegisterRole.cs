using Shuttle.Core.Contract;

namespace Shuttle.Access.Application;

public class RegisterRole(Guid id, Guid tenantId, string name, Guid auditTenantId, string auditIdentityName)
: IAuditInformation
{
    private readonly List<Guid> _permissionIds = [];

    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public Guid TenantId { get; } = Guard.AgainstEmpty(tenantId);
    public string Name { get; } = Guard.AgainstEmpty(name);
    public IEnumerable<Guid> PermissionIds => _permissionIds.AsReadOnly();
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);

    public RegisterRole AddPermissionId(Guid permissionId)
    {
        if (!_permissionIds.Contains(permissionId))
        {
            _permissionIds.Add(permissionId);
        }

        return this;
    }
}