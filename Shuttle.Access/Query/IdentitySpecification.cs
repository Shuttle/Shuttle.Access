using Shuttle.Access.SqlServer.Models;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Query;

public class IdentitySpecification : Specification<IdentitySpecification>
{
    public string Name { get; private set; } = string.Empty;
    public string NameMatch { get; private set; } = string.Empty;
    public Guid? PermissionId { get; private set; }
    public Guid? RoleId { get; private set; }
    public string RoleName { get; private set; } = string.Empty;
    public bool RolesIncluded { get; private set; }
    public DateTimeOffset? DateRegisteredStart { get; private set; }

    public IdentitySpecification IncludeRoles()
    {
        RolesIncluded = true;

        return this;
    }

    public IdentitySpecification WithName(string name)
    {
        Name = name;

        return WithMaximumRows(1);
    }

    public IdentitySpecification WithNameMatch(string nameMatch)
    {
        NameMatch = Guard.AgainstEmpty(nameMatch);
        return this;
    }

    public IdentitySpecification WithPermissionId(Guid permissionId)
    {
        PermissionId = permissionId;

        return this;
    }

    public IdentitySpecification WithRoleId(Guid roleId)
    {
        RoleId = roleId;

        return this;
    }

    public IdentitySpecification WithRoleName(string roleName)
    {
        RoleName = roleName;

        return this;
    }

    public IdentitySpecification WithDateRegisteredStart(DateTimeOffset date)
    {
        DateRegisteredStart = date;

        return this;
    }
}