using System.Collections;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Application;

public class RegisterIdentity(Guid id, string name, string description, string generatedPassword, byte[] passwordHash, string registeredBy, bool activated, Guid auditTenantId, string auditIdentityName)
    : IAuditInformation
{
    private readonly List<Guid> _roleIds = [];

    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public string Name { get; } = Guard.AgainstEmpty(name);
    public byte[] PasswordHash { get; } = passwordHash;
    public string RegisteredBy { get; } = registeredBy;
    public string Description { get; } = description;
    public bool Activated { get; } = activated;
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
    public string GeneratedPassword { get; } = generatedPassword;
    public IEnumerable<Guid> RoleIds => _roleIds;

    public RegisterIdentity AddRoleId(Guid roleId)
    {
        if (!_roleIds.Contains(roleId))
        {
            _roleIds.Add(roleId);
        }
        return this;
    }
}