using Shuttle.Core.Contract;

namespace Shuttle.Access.Query;

public class Session
{
    public Guid Id { get; set; }
    public DateTimeOffset DateRegistered { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public Guid IdentityId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
    public string IdentityDescription { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public byte[] TokenHash { get; set; } = new byte[32];
    public List<Permission> Permissions { get; set; } = [];

    public class Specification : Specification<Specification>
    {
        private readonly List<string> _permissions = [];
        public Guid? Id { get; private set; }
        public Guid? IdentityId { get; private set; }
        public Guid? RoleId { get; private set; }
        public string? IdentityName { get; private set; }
        public string? IdentityNameMatch { get; private set; }
        public IEnumerable<string> Permissions => _permissions.AsReadOnly();
        public bool HasNullTenantId { get; private set; }
        public Guid? TenantId { get; private set; }
        public byte[]? TokenHash { get; private set; }
        public Guid? Token { get; private set; }

        public Specification AddPermission(string permission)
        {
            Guard.AgainstEmpty(permission);

            if (!_permissions.Contains(permission))
            {
                _permissions.Add(permission);
            }

            return this;
        }

        public Specification AddPermissions(IEnumerable<string> permissions)
        {
            Guard.AgainstNull(permissions);

            foreach (var permission in permissions)
            {
                AddPermission(permission);
            }

            return this;
        }

        public Specification WithId(Guid id)
        {
            Id = Guard.AgainstEmpty(id);
            return this;
        }

        public Specification WithIdentityId(Guid identityId)
        {
            IdentityId = identityId;
            return this;
        }

        public Specification WithRoleId(Guid roleId)
        {
            RoleId = roleId;
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

        public Specification WithTenantId(Guid tenantId)
        {
            TenantId = Guard.AgainstEmpty(tenantId);
            return this;
        }

        public Specification WithoutTenantId()
        {
            HasNullTenantId = true;
            return this;
        }

        public Specification WithTokenHash(byte[] tokenHash)
        {
            TokenHash = Guard.AgainstNull(tokenHash);
            WithMaximumRows(1);
            return this;
        }

        public Specification WithToken(Guid token)
        {
            Token = Guard.AgainstEmpty(token);
            WithMaximumRows(1);
            return this;
        }
    }
}