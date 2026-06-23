namespace Shuttle.Access.WebApi.Contracts.v1;

public class Session
{
    public DateTimeOffset DateRegistered { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public string IdentityDescription { get; set; } = string.Empty;
    public Guid IdentityId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
    public List<SessionPermission> Permissions { get; set; } = [];
    public List<SessionToken> Tokens { get; set; } = [];
    public Guid Id { get; set; }

    public class SessionPermission
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid TenantId { get; set; }
    }

    public class SessionToken
    {
        public Guid Id { get; set; }
        public DateTimeOffset DateRegistered { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public string Application { get; set; } = "Access";
    }

    public class Specification
    {
        public List<Guid> Ids { get; set; } = [];
        public Guid? IdentityId { get; set; }
        public string IdentityName { get; set; } = string.Empty;
        public string IdentityNameMatch { get; set; } = string.Empty;
        public Guid? Token { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public string Application { get; set; } = string.Empty;
    }
}