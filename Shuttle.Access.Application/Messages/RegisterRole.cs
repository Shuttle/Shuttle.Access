using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Application;

public class RegisterRole(Guid tenantId, string name, string auditIdentityName)
{
    private readonly List<RegisterPermission> _permissions = [];
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);

    public bool HasMissingPermissions { get; private set; }

    public string Name { get; } = Guard.AgainstEmpty(name);
    public Guid TenantId { get; } = Guard.AgainstEmpty(tenantId);

    public RegisterRole AddPermissions(IEnumerable<RegisterPermission> permissions)
    {
        Guard.AgainstNull(permissions);

        foreach (var permission in permissions)
        {
            if (_permissions.All(item => !item.Name.Equals(permission.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                _permissions.Add(permission);
            }
        }

        return this;
    }

    public IEnumerable<RegisterPermission> GetPermissions()
    {
        return _permissions.AsReadOnly();
    }

    public RegisterRole MissingPermissions()
    {
        HasMissingPermissions = true;
        return this;
    }
}