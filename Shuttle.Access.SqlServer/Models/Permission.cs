using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(Permission), Schema = "access")]
[PrimaryKey(nameof(Id))]
[Index(nameof(Name), IsUnique = true, Name = $"UX_{nameof(Permission)}_{nameof(Name)}")]
public class Permission
{
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    public ICollection<PermissionTenant> PermissionTenants { get; set; } = [];

    [Required]
    public int Status { get; set; }
}