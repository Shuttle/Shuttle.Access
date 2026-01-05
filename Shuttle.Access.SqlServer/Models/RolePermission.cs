using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.SqlServer.Models;

[PrimaryKey(nameof(RoleId), nameof(PermissionId))]
public class RolePermission
{
    [Required]
    public DateTimeOffset DateRegistered { get; set; }

    public Permission Permission { get; set; } = null!;

    [Required]
    public Guid PermissionId { get; set; }

    public Role Role { get; set; } = null!;

    [Required]
    public Guid RoleId { get; set; }
}