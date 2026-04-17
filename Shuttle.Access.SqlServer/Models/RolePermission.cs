using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(RolePermission), Schema = "access")]
[PrimaryKey(nameof(TenantId), nameof(RoleId), nameof(PermissionId))]
public class RolePermission
{
    public Permission Permission { get; set; } = null!;

    [Required]
    public Guid PermissionId { get; set; }

    public Role Role { get; set; } = null!;

    [Required]
    public Guid RoleId { get; set; }

    [Required]
    public Guid TenantId { get; set; }
}