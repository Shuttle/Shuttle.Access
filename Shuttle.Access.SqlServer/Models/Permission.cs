using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.SqlServer.Models;

[Index(nameof(Name), IsUnique = true, Name = $"UX_{nameof(Permission)}_{nameof(Name)}")]
public class Permission
{
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];

    [Required]
    public int Status { get; set; }

    public class Specification
    {
        private readonly List<Guid> _ids = new();
        private readonly List<string> _names = new();
        private readonly List<Guid> _roleIds = new();
        private readonly List<int> _statuses = new();
        public IEnumerable<Guid> Ids => _ids.AsReadOnly();
        public int MaximumRows { get; private set; }

        public string NameMatch { get; private set; } = string.Empty;
        public IEnumerable<string> Names => _names.AsReadOnly();
        public IEnumerable<Guid> RoleIds => _roleIds.AsReadOnly();
        public IEnumerable<int> Statuses => _statuses.AsReadOnly();

        public Specification AddId(Guid id)
        {
            if (!_ids.Contains(id))
            {
                _ids.Add(id);
            }

            return this;
        }

        public Specification AddIds(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                AddId(id);
            }

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

        public Specification WithMaximumRows(int maximumRows)
        {
            MaximumRows = maximumRows;

            return this;
        }

        public Specification WithNameMatch(string nameMatch)
        {
            NameMatch = nameMatch;

            return this;
        }
    }
}