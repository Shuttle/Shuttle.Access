namespace Shuttle.Access.WebApi.Contracts.v1;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public List<Permission> Permissions { get; set; } = [];
    public List<Identity> Identities { get; set; } = [];

    public class Identity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Specification
    {
        public string NameMatch { get; set; } = string.Empty;
        public bool ShouldIncludePermissions { get; set; }
    }
}