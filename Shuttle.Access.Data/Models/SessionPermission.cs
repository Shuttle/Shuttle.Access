using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.Data.Models;

[PrimaryKey(nameof(IdentityId), nameof(PermissionId))]
public class SessionPermission
{
    [Required]
    public Guid IdentityId { get; set; }

    [Required]
    public Guid PermissionId { get; set; }
}