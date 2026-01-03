using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.Data.Models;

[PrimaryKey(nameof(IdentityId), nameof(RoleId))]
public class IdentityRole
{
    [Required]
    public DateTimeOffset DateRegistered { get; set; } = DateTimeOffset.UtcNow;

    [ForeignKey(nameof(IdentityId))]
    public Identity Identity { get; set; } = null!;

    public Guid IdentityId { get; set; }

    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; } = null!;

    public Guid RoleId { get; set; }
}