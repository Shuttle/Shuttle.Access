using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.Data.Models;

[Index(nameof(Name), IsUnique = true, Name = $"UX_{nameof(Role)}_{nameof(Name)}")]
public class Role
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
}