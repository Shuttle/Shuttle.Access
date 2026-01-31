using Shuttle.Access.SqlServer.Models;

namespace Shuttle.Access.Query;

public class PermissionSpecification : Specification<PermissionSpecification>
{
    private readonly List<string> _names = [];
    private readonly List<Guid> _roleIds = [];
    private readonly List<int> _statuses = [];

    public string NameMatch { get; private set; } = string.Empty;
    public IEnumerable<string> Names => _names.AsReadOnly();
    public IEnumerable<Guid> RoleIds => _roleIds.AsReadOnly();
    public IEnumerable<int> Statuses => _statuses.AsReadOnly();

    public PermissionSpecification AddName(string name)
    {
        if (!_names.Contains(name))
        {
            _names.Add(name);
        }

        return this;
    }

    public PermissionSpecification AddRoleId(Guid roleId)
    {
        if (!_roleIds.Contains(roleId))
        {
            _roleIds.Add(roleId);
        }

        return this;
    }

    public PermissionSpecification AddStatus(int status)
    {
        if (!_statuses.Contains(status))
        {
            _statuses.Add(status);
        }

        return this;
    }

    public PermissionSpecification WithNameMatch(string nameMatch)
    {
        NameMatch = nameMatch;

        return this;
    }
}