using Shuttle.Core.Contract;

namespace Shuttle.Access.Query;

public class Identity
{
    public DateTimeOffset? DateActivated { get; set; }
    public DateTimeOffset DateRegistered { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? GeneratedPassword { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegisteredBy { get; set; } = string.Empty;
    public List<Tenant> Tenants { get; set; } = [];
    public List<Role> Roles { get; set; } = [];

    public class Specification : Specification<Specification>
    {
        public DateTimeOffset? DateRegisteredStart { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string NameMatch { get; private set; } = string.Empty;
        public Guid? PermissionId { get; private set; }
        public Guid? RoleId { get; private set; }
        public string RoleName { get; private set; } = string.Empty;
        public bool ShouldIncludeRoles { get; private set; }
        public Guid? TenantId { get; private set; }
        public bool ShouldIncludePermissions { get; private set; }
        public bool ShouldIncludeTenants { get; private set; }

        public Specification IncludeRoles()
        {
            ShouldIncludeRoles = true;

            return this;
        }

        public Specification WithDateRegisteredStart(DateTimeOffset date)
        {
            DateRegisteredStart = date;

            return this;
        }

        public Specification WithName(string name)
        {
            Name = name;

            return WithMaximumRows(1);
        }

        public Specification WithNameMatch(string nameMatch)
        {
            NameMatch = Guard.AgainstEmpty(nameMatch);
            return this;
        }

        public Specification WithPermissionId(Guid permissionId)
        {
            PermissionId = permissionId;

            return this;
        }

        public Specification WithRoleId(Guid roleId)
        {
            RoleId = roleId;

            return this;
        }

        public Specification WithRoleName(string roleName)
        {
            RoleName = roleName;

            return this;
        }

        public Specification WithTenantId(Guid tenantId)
        {
            TenantId = Guard.AgainstEmpty(tenantId);

            return this;
        }

        public Specification IncludePermissions()
        {
            ShouldIncludePermissions = true;

            return this;
        }

        public Specification IncludeTenants()
        {
            ShouldIncludeTenants = true;

            return this;
        }
    }
}