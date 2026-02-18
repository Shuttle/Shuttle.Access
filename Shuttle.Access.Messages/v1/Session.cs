namespace Shuttle.Access.Messages.v1;

public class Session
{
    public DateTimeOffset DateRegistered { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public string IdentityDescription { get; set; } = string.Empty;
    public Guid IdentityId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
    public Guid? TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public List<SessionTenants.Tenant> Tenants { get; set; }

    public class Specification
    {
        public Guid? Id { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? IdentityId { get; set; }
        public string IdentityName { get; set; } = string.Empty;
        public string IdentityNameMatch { get; set; } = string.Empty;
        public bool ShouldIncludePermissions { get; set; }
        public Guid? Token { get; set; }
    }
}