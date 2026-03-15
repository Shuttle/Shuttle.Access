namespace Shuttle.Access.Query;

public class Permission
{
    public string Description { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PermissionStatus Status { get; set; }

    public class Specification : Specification<Specification>
    {
        private readonly List<string> _names = [];
        private readonly List<Guid> _roleIds = [];
        private readonly List<int> _statuses = [];

        public string NameMatch { get; private set; } = string.Empty;
        public IEnumerable<string> Names => _names.AsReadOnly();
        public IEnumerable<Guid> RoleIds => _roleIds.AsReadOnly();
        public IEnumerable<int> Statuses => _statuses.AsReadOnly();

        public Specification AddName(string name)
        {
            if (!_names.Contains(name))
            {
                _names.Add(name);
            }

            return this;
        }

        public Specification AddRoleId(Guid roleId)
        {
            if (!_roleIds.Contains(roleId))
            {
                _roleIds.Add(roleId);
            }

            return this;
        }

        public Specification AddStatus(int status)
        {
            if (!_statuses.Contains(status))
            {
                _statuses.Add(status);
            }

            return this;
        }

        public Specification WithNameMatch(string nameMatch)
        {
            NameMatch = nameMatch;

            return this;
        }
    }
}