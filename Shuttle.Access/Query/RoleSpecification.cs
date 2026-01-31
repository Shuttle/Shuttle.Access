using Shuttle.Access.SqlServer.Models;

namespace Shuttle.Access.Query;

public class RoleSpecification : Specification<RoleSpecification>
{
    private readonly List<string> _names = [];
    private readonly List<Guid> _permissionIds = [];
    public string NameMatch { get; private set; } = string.Empty;
    public IEnumerable<string> Names => _names.AsReadOnly();
    public IEnumerable<Guid> PermissionIds => _permissionIds.AsReadOnly();
    public bool PermissionsIncluded { get; private set; }

    public RoleSpecification AddName(string name)
    {
        if (!_names.Contains(name))
        {
            _names.Add(name);
        }

        return this;
    }

    public RoleSpecification AddPermissionId(Guid id)
    {
        if (!_permissionIds.Contains(id))
        {
            _permissionIds.Add(id);
        }

        return this;
    }

    public RoleSpecification AddPermissionIds(IEnumerable<Guid> ids)
    {
        foreach (var id in ids)
        {
            AddPermissionId(id);
        }

        return this;
    }

    public RoleSpecification IncludePermissions()
    {
        PermissionsIncluded = true;

        return this;
    }

    public RoleSpecification WithNameMatch(string nameMatch)
    {
        NameMatch = nameMatch;

        return this;
    }
}