using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Application;

public class RegisterRole(string name)
{
    private readonly List<RegisterPermission> _permissions = [];

    public bool HasMissingPermissions { get; private set; }

    public string Name { get; } = Guard.AgainstEmpty(name);

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