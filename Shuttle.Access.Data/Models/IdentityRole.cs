using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.Data.Models;

[PrimaryKey(nameof(IdentityId), nameof(RoleId))]
public class IdentityRole
{
    [Required]
    public DateTimeOffset DateRegistered { get; set; } = DateTimeOffset.UtcNow;

    public Guid IdentityId { get; set; }
    public Guid RoleId { get; set; }
}