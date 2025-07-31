using Shuttle.Core.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Shuttle.Access;

public class Session
{
    public class Permission
    {
        public Guid Id { get; }
        public string Name { get; }

        public Permission(Guid id, string name)
        {
            Id = Guard.AgainstEmpty(id);
            Name = Guard.AgainstEmpty(name);
        }
    }

    private readonly List<Permission> _permissions = [];

    public Session(byte[] token, Guid identityId, string identityName, DateTimeOffset dateRegistered, DateTimeOffset expiryDate)
    {
        Token = Guard.AgainstNull(token);
        IdentityId = Guard.AgainstEmpty(identityId);
        IdentityName = Guard.AgainstEmpty(identityName);
        DateRegistered = dateRegistered;
        ExpiryDate = expiryDate;
    }

    public DateTimeOffset DateRegistered { get; set; }

    public DateTimeOffset ExpiryDate { get; private set; }

    public bool HasExpired => DateTimeOffset.UtcNow >= ExpiryDate;
    public Guid IdentityId { get; }
    public string IdentityName { get; }

    public IEnumerable<Permission> Permissions => _permissions.AsReadOnly();
    public byte[] Token { get; private set; }
    public bool HasPermissions => _permissions.Any();

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
}