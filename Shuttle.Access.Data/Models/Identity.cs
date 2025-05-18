using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

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
}