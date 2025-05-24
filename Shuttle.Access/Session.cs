using Shuttle.Core.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Shuttle.Access;

public class Session
{
    private readonly List<string> _permissions = [];

    public Session(byte[] token, Guid identityId, string identityName, DateTimeOffset dateRegistered, DateTimeOffset expiryDate)
    {
        Guard.AgainstEmpty(identityName);

        Token = Guard.AgainstNull(token);
        IdentityId = identityId;
        IdentityName = identityName;
        DateRegistered = dateRegistered;
        ExpiryDate = expiryDate;
    }

    public DateTimeOffset DateRegistered { get; set; }

    public DateTimeOffset ExpiryDate { get; private set; }

    public bool HasExpired => DateTimeOffset.UtcNow >= ExpiryDate;
    public Guid IdentityId { get; }
    public string IdentityName { get; }

    public IEnumerable<string> Permissions => _permissions.AsReadOnly();
    public byte[] Token { get; private set; }
    public bool HasPermissions => _permissions.Any();

    public Session AddPermission(string permission)
    {
        Guard.AgainstEmpty(permission);

        if (!_permissions.Contains(permission))
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
                Regex.IsMatch(requiredPermission, $"^{Regex.Escape(permission).Replace(@"\*", ".*")}$", RegexOptions.IgnoreCase));
    }

    public void Renew(DateTimeOffset expiryDate, byte[] token)
    {
        Token = Guard.AgainstNull(token);
        ExpiryDate = expiryDate;
    }
}