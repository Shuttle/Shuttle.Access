using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;
using System.ComponentModel.DataAnnotations;

namespace Shuttle.Access.Data.Models;

[Index(nameof(Name), IsUnique = true, Name = $"UX_{nameof(Identity)}_{nameof(Name)}")]
public class Identity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(320)]
    public string Name { get; set; } = string.Empty;    

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTimeOffset DateRegistered { get; set; }

    [Required]
    [StringLength(320)]
    public string RegisteredBy { get; set; } = string.Empty;

    [StringLength(65)]
    public string? GeneratedPassword { get; set; }

    public DateTimeOffset? DateActivated { get; set; }

    public ICollection<IdentityRole> IdentityRoles { get; set; } = [];

    public class Specification
    {
        public Guid? Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string NameMatch { get; private set; } = string.Empty;
        public Guid? PermissionId { get; private set; }
        public Guid? RoleId { get; private set; }
        public string RoleName { get; private set; } = string.Empty;
        public bool RolesIncluded { get; private set; }
        public DateTimeOffset? StartDateRegistered { get; private set; }
        public int MaximumRows { get; private set; }

        public Specification IncludeRoles()
        {
            RolesIncluded = true;

            return this;
        }

        public Specification WithIdentityId(Guid id)
        {
            Id = id;
            MaximumRows = 1;

            return this;
        }

        public Specification WithName(string name)
        {
            Name = name;
            MaximumRows = 1;

            return this;
        }

        public Specification WithPermissionId(Guid permissionId)
        {
            PermissionId = permissionId;

            return this;
        }

        public Specification WithRoleId(Guid roleId)
        {
            RoleId = roleId;

            return this;
        }

        public Specification WithRoleName(string roleName)
        {
            RoleName = roleName;

            return this;
        }

        public Specification WithStartDateRegistered(DateTimeOffset date)
        {
            StartDateRegistered = date;

            return this;
        }

        public Specification WithMaximumRows(int maximumRows)
        {
            MaximumRows = maximumRows;

            return this;
        }

        public Specification WithNameMatch(string nameMatch)
        {
            NameMatch = Guard.AgainstEmpty(nameMatch);
            return this;
        }
    }
}