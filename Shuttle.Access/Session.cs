using System.Text.RegularExpressions;
using Shuttle.Core.Contract;

namespace Shuttle.Access;

public class Session(Guid id, byte[] token, Guid identityId, string identityName, DateTimeOffset dateRegistered, DateTimeOffset expiryDate)
{
    private readonly List<Permission> _permissions = [];
    public DateTimeOffset DateRegistered { get; set; } = dateRegistered;

    public DateTimeOffset ExpiryDate { get; private set; } = expiryDate;

    public bool HasExpired => DateTimeOffset.UtcNow >= ExpiryDate;
    public bool HasPermissions => _permissions.Count > 0;
    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public Guid IdentityId { get; } = Guard.AgainstEmpty(identityId);
    public string IdentityName { get; } = Guard.AgainstEmpty(identityName);

    public IEnumerable<Permission> Permissions => _permissions.AsReadOnly();

    public Guid? TenantId { get; private set; }

    public byte[] Token { get; private set; } = Guard.AgainstNull(token);

    public Session AddPermission(Permission permission)
    {
        Guard.AgainstNull(permission);

        if (_permissions.All(item => item.Id != permission.Id))
        {
            _permissions.Add(permission);
        }

        return this;
    }

    public void ClearPermissions()
    {
        _permissions.Clear();
    }

    public bool HasPermission(string requiredPermission)
    {
        Guard.AgainstEmpty(requiredPermission);

        return _permissions
            .Any(permission =>
                Regex.IsMatch(requiredPermission, $"^{Regex.Escape(permission.Name).Replace(@"\*", ".*")}$", RegexOptions.IgnoreCase));
    }

    public void Renew(DateTimeOffset expiryDate, byte[] token)
    {
        Token = Guard.AgainstNull(token);
        ExpiryDate = expiryDate;
    }

    public Session WithTenantId(Guid tenantId)
    {
        TenantId = Guard.AgainstEmpty(tenantId);
        return this;
    }

    public class Permission(Guid id, string name)
    {
        public Guid Id { get; } = Guard.AgainstEmpty(id);
        public string Name { get; } = Guard.AgainstEmpty(name);
    }
}