using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(Role), Schema = "access")]
[PrimaryKey(nameof(TenantId), nameof(Id))]
[Index(nameof(TenantId), nameof(Name), IsUnique = true, Name = $"UX_{nameof(Role)}_{nameof(TenantId)}_{nameof(Name)}")]
[Index(nameof(Id), IsUnique = true, Name = $"UX_{nameof(Role)}_{nameof(Id)}")]
public class Role
{
    [Required]
    public Guid TenantId { get; set; }

    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    public ICollection<IdentityRole> IdentityRoles { get; set; } = [];
}