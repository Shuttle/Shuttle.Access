using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(IdentityTenant), Schema = "access")]
[PrimaryKey(nameof(IdentityId), nameof(TenantId))]
public class IdentityTenant
{
    public Identity Identity { get; set; } = null!;
    public Guid IdentityId { get; set; }

    [Required]
    public DateTimeOffset RegisteredAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    [StringLength(320)]
    public string RegisteredBy { get; set; } = string.Empty;

    public Tenant Tenant { get; set; } = null!;
    public Guid TenantId { get; set; }

    [Required]
    public int Status { get; set; }
}