using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.Data.Models;

[PrimaryKey(nameof(IdentityId), nameof(PermissionId))]
public class SessionPermission
{
    [Required]
    public Guid IdentityId { get; set; }

    [Required]
    public Guid PermissionId { get; set; }

    [ForeignKey(nameof(IdentityId))]
    public Session Session { get; set; } = null!;


    [ForeignKey(nameof(PermissionId))]
    public Permission Permission { get; set; } = null!;
}