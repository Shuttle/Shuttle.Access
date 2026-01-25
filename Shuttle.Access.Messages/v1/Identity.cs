namespace Shuttle.Access.Messages.v1;

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
        public string NameMatch { get; set; } = string.Empty;
        public bool ShouldIncludeRoles { get; set; }
    }
}