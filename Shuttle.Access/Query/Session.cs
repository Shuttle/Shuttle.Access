using Shuttle.Contract;

namespace Shuttle.Access.Query;

public class Session
{
    public DateTimeOffset DateRegistered { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public Guid Id { get; set; }
    public string IdentityDescription { get; set; } = string.Empty;
    public Guid IdentityId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
    public List<Permission> Permissions { get; set; } = [];
    public string TokenHash { get; set; } = string.Empty;
    public string Application { get; set; } = "Access";

    public class Permission
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid TenantId { get; set; }
    }

    public class Specification : Specification<Specification>
    {
        private readonly List<string> _permissions = [];

        public bool HasCriteria =>
            HasIds ||
            HasExcludedIds ||
            IdentityId != null ||
            RoleId != null ||
            !string.IsNullOrWhiteSpace(Application) ||
            !string.IsNullOrWhiteSpace(IdentityName) ||
            !string.IsNullOrWhiteSpace(IdentityNameMatch) ||
            _permissions.Count > 0 ||
            !string.IsNullOrWhiteSpace(TokenHash) ||
            Token != null;

        public Guid? IdentityId { get; private set; }
        public string? IdentityName { get; private set; }
        public string? IdentityNameMatch { get; private set; }
        public IEnumerable<string> Permissions => _permissions.AsReadOnly();
        public Guid? RoleId { get; private set; }
        public Guid? Token { get; private set; }
        public string TokenHash { get; private set; } = string.Empty;
        public string Application { get; private set; } = string.Empty;

        public Specification AddPermissions(IEnumerable<string> permissions)
        {
            Guard.AgainstNull(permissions);

            foreach (var permission in permissions)
            {
                if (!_permissions.Contains(permission))
                {
                    _permissions.Add(permission);
                }
            }

            return this;
        }

        public Specification WithIdentityId(Guid identityId)
        {
            IdentityId = identityId;
            return this;
        }

        public Specification WithIdentityName(string identityName)
        {
            IdentityName = Guard.AgainstEmpty(identityName);
            return this;
        }

        public Specification WithIdentityNameMatch(string identityNameMatch)
        {
            IdentityNameMatch = Guard.AgainstEmpty(identityNameMatch);

            return this;
        }

        public Specification WithRoleId(Guid roleId)
        {
            RoleId = roleId;
            return this;
        }

        public Specification WithToken(Guid token)
        {
            Token = Guard.AgainstEmpty(token);
            WithMaximumRows(1);
            return this;
        }

        public Specification WithTokenHash(byte[] tokenHash)
        {
            return WithTokenHash(Convert.ToHexString(tokenHash));
        }

        public Specification WithTokenHash(string tokenHash)
        {
            TokenHash = Guard.AgainstEmpty(tokenHash);
            WithMaximumRows(1);
            return this;
        }

        public Specification WithApplication(string application)
        {
            Application = Guard.AgainstEmpty(application);
            return this;
        }
    }
}