using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.SqlServer.Models;

[PrimaryKey(nameof(IdentityId), nameof(RoleId))]
public class IdentityRole
{
    [Required]
    public DateTimeOffset DateRegistered { get; set; } = DateTimeOffset.UtcNow;

    public Identity Identity { get; set; } = null!;

    public Guid IdentityId { get; set; }

    public Role Role { get; set; } = null!;

    public Guid RoleId { get; set; }
}