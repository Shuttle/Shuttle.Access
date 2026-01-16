using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(IdentityRole), Schema = "access")]
[PrimaryKey(nameof(TenantId), nameof(IdentityId), nameof(RoleId))]
public class IdentityRole
{
    public Identity Identity { get; set; } = null!;

    [Required]
    public Guid IdentityId { get; set; }

    public Role Role { get; set; } = null!;

    [Required]
    public Guid RoleId { get; set; }

    [Required]
    public Guid TenantId { get; set; }
}