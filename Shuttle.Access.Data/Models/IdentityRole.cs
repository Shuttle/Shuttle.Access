using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.Data.Models;

[PrimaryKey(nameof(IdentityId), nameof(RoleId))]
public class IdentityRole
{
    [Required]
    public DateTimeOffset DateRegistered { get; set; } = DateTimeOffset.UtcNow;

    public Guid IdentityId { get; set; }
    public Guid RoleId { get; set; }

    [ForeignKey(nameof(IdentityId))]
    public Identity Identity { get; set; } = null!;
    
    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; } = null!;
}