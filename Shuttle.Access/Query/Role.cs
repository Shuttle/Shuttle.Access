using Shuttle.Contract;

namespace Shuttle.Access.Query;

public class Role
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<Permission> Permissions { get; set; } = [];
    public List<Identity> Identities { get; set; } = [];

    public class Identity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Specification : Specification<Specification>
    {
        public Guid? TenantId { get; private set; }
        private readonly List<string> _names = [];
        private readonly List<Guid> _permissionIds = [];
        public string NameMatch { get; private set; } = string.Empty;
        public IEnumerable<string> Names => _names.AsReadOnly();
        public IEnumerable<Guid> PermissionIds => _permissionIds.AsReadOnly();
        public bool PermissionsIncluded { get; private set; }

        public Specification WithTenantId(Guid tenantId)
        {
            TenantId = Guard.AgainstEmpty(tenantId);
            return this;
        }

        public Specification AddName(string name)
        {
            if (!_names.Contains(name))
            {
                _names.Add(name);
            }

            return this;
        }

        public Specification AddPermissionId(Guid id)
        {
            if (!_permissionIds.Contains(id))
            {
                _permissionIds.Add(id);
            }

            return this;
        }

        public Specification AddPermissionIds(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                AddPermissionId(id);
            }

            return this;
        }

        public Specification IncludePermissions()
        {
            PermissionsIncluded = true;

            return this;
        }

        public Specification WithNameMatch(string nameMatch)
        {
            NameMatch = nameMatch;

            return this;
        }
    }
}