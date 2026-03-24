namespace Shuttle.Access.WebApi.Contracts.v1;

public class Identity
{
    public DateTimeOffset? DateActivated { get; set; }
    public DateTimeOffset DateRegistered { get; set; }
    public string Description { get; set; } = string.Empty;
    public string GeneratedPassword { get; set; } = string.Empty;

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegisteredBy { get; set; } = string.Empty;
    public List<Role> Roles { get; set; } = [];
    public List<Tenant> Tenants { get; set; } = [];

    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Tenant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Specification
    {
        public List<Guid> Ids { get; set; } = [];
        public string NameMatch { get; set; } = string.Empty;
        public bool ShouldIncludePermissions { get; set; }
        public bool ShouldIncludeRoles { get; set; }
        public bool ShouldIncludeTenants { get; set; }
    }
}