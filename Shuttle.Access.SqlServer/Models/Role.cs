using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(Role), Schema = "access")]
[PrimaryKey(nameof(TenantId), nameof(Id))]
[Index(nameof(Name), IsUnique = true, Name = $"UX_{nameof(Role)}_{nameof(Name)}")]
public class Role
{
    [Required]
    public Guid TenantId { get; set; }

    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];

    public class Specification
    {
        private readonly List<string> _names = [];
        private readonly List<Guid> _permissionIds = [];
        private readonly List<Guid> _rolesIds = [];
        public int MaximumRows { get; private set; }

        public string NameMatch { get; private set; } = string.Empty;
        public IEnumerable<string> Names => _names.AsReadOnly();
        public IEnumerable<Guid> PermissionIds => _permissionIds.AsReadOnly();
        public bool PermissionsIncluded { get; private set; }
        public IEnumerable<Guid> RoleIds => _rolesIds.AsReadOnly();

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

        public Specification AddRoleId(Guid id)
        {
            if (!_rolesIds.Contains(id))
            {
                _rolesIds.Add(id);
            }

            return this;
        }

        public Specification AddRoleIds(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                AddRoleId(id);
            }

            return this;
        }

        public Specification IncludePermissions()
        {
            PermissionsIncluded = true;

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