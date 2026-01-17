using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access;

public class Role
{
    private readonly List<Guid> _permissionIds = [];
    public string Name { get; private set; } = string.Empty;

    public Guid TenantId { get; private set; }

    public PermissionAdded AddPermission(Guid permissionId)
    {
        if (HasPermission(permissionId))
        {
            throw new InvalidOperationException(string.Format(Resources.DuplicateRolePermissionException, permissionId,
                Name));
        }

        return On(new PermissionAdded { PermissionId = permissionId });
    }

    public bool HasPermission(Guid permissionId)
    {
        return _permissionIds.Contains(permissionId);
    }

    public static string Key(string name)
    {
        return $"[role]:name={name}";
    }

    private Registered On(Registered registered)
    {
        Guard.AgainstNull(registered);

        Name = registered.Name;

        return registered;
    }

    private Shuttle.Access.Events.Role.v2.Registered On(Shuttle.Access.Events.Role.v2.Registered registered)
    {
        Guard.AgainstNull(registered);

        TenantId = registered.TenantId;
        Name = registered.Name;

        return registered;
    }

    private NameSet On(NameSet nameSet)
    {
        Guard.AgainstNull(nameSet);

        Name = nameSet.Name;

        return nameSet;
    }

    private PermissionAdded On(PermissionAdded permissionAdded)
    {
        Guard.AgainstNull(permissionAdded);

        _permissionIds.Add(permissionAdded.PermissionId);

        return permissionAdded;
    }

    private PermissionRemoved On(PermissionRemoved permissionRemoved)
    {
        Guard.AgainstNull(permissionRemoved);

        _permissionIds.Remove(permissionRemoved.PermissionId);

        return permissionRemoved;
    }

    private Removed On(Removed removed)
    {
        Guard.AgainstNull(removed);

        return removed;
    }

    public Shuttle.Access.Events.Role.v2.Registered Register(Guid tenantId, string name)
    {
        return On(new Shuttle.Access.Events.Role.v2.Registered
        {
            TenantId = tenantId,
            Name = name
        });
    }

    public Removed Remove()
    {
        return On(new Removed());
    }

    public PermissionRemoved RemovePermission(Guid permissionId)
    {
        if (!HasPermission(permissionId))
        {
            throw new InvalidOperationException(string.Format(Resources.PermissionNotFoundException, permissionId,
                Name));
        }

        return On(new PermissionRemoved { PermissionId = permissionId });
    }

    public NameSet SetName(string name)
    {
        if (name.Equals(Name))
        {
            throw new DomainException(string.Format(Resources.PropertyUnchangedException, "Name", Name));
        }

        return On(new NameSet
        {
            Name = name
        });
    }
}