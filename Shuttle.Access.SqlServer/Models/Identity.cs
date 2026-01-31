using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(Identity), Schema = "access")]
[Index(nameof(Name), IsUnique = true, Name = $"UX_{nameof(Identity)}_{nameof(Name)}")]
public class Identity
{
    public DateTimeOffset? DateActivated { get; set; }

    [Required]
    public DateTimeOffset DateRegistered { get; set; }

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(65)]
    public string? GeneratedPassword { get; set; }

    [Key]
    public Guid Id { get; set; }

    public ICollection<IdentityRole> IdentityRoles { get; set; } = [];
    public ICollection<IdentityTenant> IdentityTenants { get; set; } = [];

    [Required]
    [StringLength(320)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(320)]
    public string RegisteredBy { get; set; } = string.Empty;
}