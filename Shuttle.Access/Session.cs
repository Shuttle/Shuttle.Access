using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;

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

        if (!HasPermission(permission))
        {
            _permissions.Add(permission);
        }

        return this;
    }

    public void ClearPermissions()
    {
        _permissions.Clear();
    }

    public bool HasPermission(string permission)
    {
        return _permissions.Find(
            candidate => candidate.Equals(permission, StringComparison.InvariantCultureIgnoreCase)) != null || _permissions.Contains("*");
    }

    public void Renew(DateTimeOffset expiryDate, byte[] token)
    {
        Token = Guard.AgainstNull(token);
        ExpiryDate = expiryDate;
    }
}