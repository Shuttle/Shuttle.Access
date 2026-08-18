using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class RegisterRole(Guid id, Guid tenantId, string name, Guid auditTenantId, string auditIdentityName)
: IAuditInformation
{
    private readonly List<Guid> _permissionIds = [];
    private readonly List<string> _permissionNames = [];

    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public Guid TenantId { get; } = Guard.AgainstEmpty(tenantId);
    public string Name { get; } = Guard.AgainstEmpty(name);
    public IEnumerable<Guid> PermissionIds => _permissionIds.AsReadOnly();

    /// <summary>
    /// Permission names to resolve to ids before registration, tolerating the case where the permission has
    /// only just been registered elsewhere and has not yet appeared in the read model.
    /// </summary>
    public IEnumerable<string> PermissionNames => _permissionNames.AsReadOnly();

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

    public RegisterRole AddPermissionName(string name)
    {
        if (!_permissionNames.Contains(name))
        {
            _permissionNames.Add(name);
        }

        return this;
    }
}