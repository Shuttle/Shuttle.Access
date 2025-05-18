using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Application;

public class RegisterRole
{
    private readonly List<string> _permissions = [];

    public string Name { get; }
    public bool HasMissingPermissions { get; private set; }
    
    public RegisterRole(string name)
    {
        Name = Guard.AgainstEmpty(name);
    }

    public RegisterRole AddPermissions(IEnumerable<string> permissions)
    {
        Guard.AgainstNull(permissions);

        foreach (var permission in permissions)
        {
            if (!_permissions.Contains(permission))
            {
                _permissions.Add(permission);
            }
        }

        return this;
    }

    public IEnumerable<string> GetPermissions() => _permissions.AsReadOnly();

    public RegisterRole MissingPermissions()
    {
        HasMissingPermissions = true;
        return this;
    }
}