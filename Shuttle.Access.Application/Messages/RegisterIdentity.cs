using Shuttle.Core.Contract;

namespace Shuttle.Access.Application;

public class RegisterIdentity(Guid id, string name, string description, string generatedPassword, byte[] passwordHash, string registeredBy, bool activated, Guid auditTenantId, string auditIdentityName)
    : IAuditInformation
{
    private readonly List<Guid> _roleIds = [];
    private readonly List<Guid> _tenantIds = [];

    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public string Name { get; } = Guard.AgainstEmpty(name);
    public byte[] PasswordHash { get; } = passwordHash;
    public string RegisteredBy { get; } = registeredBy;
    public string Description { get; } = description;
    public bool Activated { get; } = activated;
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
    public string GeneratedPassword { get; } = generatedPassword;
    public IEnumerable<Guid> RoleIds => _roleIds.AsReadOnly();
    public IEnumerable<Guid> TenantIds => _tenantIds.AsReadOnly();

    public bool HasTenantIds => _tenantIds.Count > 0;

    public RegisterIdentity AddRoleId(Guid roleId)
    {
        if (!_roleIds.Contains(roleId))
        {
            _roleIds.Add(roleId);
        }
        return this;
    }

    public RegisterIdentity AddTenantId(Guid tenantId)
    {
        if (!_tenantIds.Contains(tenantId))
        {
            _tenantIds.Add(tenantId);
        }
        return this;
    }

    public RegisterIdentity AddRoleIds(IEnumerable<Guid> roleIds)
    {
        foreach (var roleId in roleIds)
        {
            AddRoleId(roleId);
        }
        return this;
    }

    public RegisterIdentity AddTenantIds(IEnumerable<Guid> tenantIds)
    {
        foreach (var tenantId in tenantIds)
        {
            AddTenantId(tenantId);
        }
        return this;
    }
}