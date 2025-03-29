using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.Data.Models;

[PrimaryKey(nameof(IdentityId), nameof(PermissionName))]
public class SessionPermission
{
    [Required]
    public Guid IdentityId { get; set; }

    [Required]
    [StringLength(200)]
    public string PermissionName { get; set; } = string.Empty;
}