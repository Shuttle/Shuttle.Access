using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.Data.Models;

[PrimaryKey(nameof(RoleId), nameof(PermissionId))]
public class RolePermission
{
    [Required]
    public Guid RoleId { get; set; }

    [Required]
    public Guid PermissionId { get; set; }

    [Required]
    public DateTimeOffset DateRegistered { get; set; }
}