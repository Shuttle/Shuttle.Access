namespace Shuttle.Access.WebApi.Contracts.v1;

public class Session
{
    public DateTimeOffset DateRegistered { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public Guid Id { get; set; }
    public string IdentityDescription { get; set; } = string.Empty;
    public Guid IdentityId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
    public List<SessionPermission> Permissions { get; set; } = [];
    public List<SessionToken> Tokens { get; set; } = [];

    public class SessionPermission
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid TenantId { get; set; }
    }

    public class SessionToken
    {
        public string Application { get; set; } = "Access";
        public DateTimeOffset DateRegistered { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }
        public Guid Id { get; set; }
        public string TokenHash { get; set; } = string.Empty;
    }

    public class Specification
    {
        public string Application { get; set; } = string.Empty;
        public Guid? IdentityId { get; set; }
        public string IdentityName { get; set; } = string.Empty;
        public string IdentityNameMatch { get; set; } = string.Empty;
        public List<Guid> Ids { get; set; } = [];
        public Guid? Token { get; set; }
        public string TokenHash { get; set; } = string.Empty;

        public override string ToString()
        {
            var parts = new List<string>();

            if (Ids.Count > 0)
            {
                parts.Add($"Ids=[{string.Join(", ", Ids)}]");
            }

            if (IdentityId.HasValue)
            {
                parts.Add($"IdentityId={IdentityId.Value}");
            }

            if (!string.IsNullOrWhiteSpace(IdentityName))
            {
                parts.Add($"IdentityName='{IdentityName}'");
            }

            if (!string.IsNullOrWhiteSpace(IdentityNameMatch))
            {
                parts.Add($"IdentityNameMatch='{IdentityNameMatch}'");
            }

            if (Token.HasValue)
            {
                parts.Add($"Token={Token.Value}");
            }

            if (!string.IsNullOrWhiteSpace(TokenHash))
            {
                parts.Add($"TokenHash='{TokenHash}'");
            }

            if (!string.IsNullOrWhiteSpace(Application))
            {
                parts.Add($"Application='{Application}'");
            }

            return parts.Count > 0
                ? $"{nameof(Specification)} {{ {string.Join(", ", parts)} }}"
                : $"{nameof(Specification)} {{ <no filters> }}";
        }
    }
}