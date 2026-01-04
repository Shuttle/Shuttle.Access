using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.SqlServer.Models;

[PrimaryKey(nameof(IdentityId), nameof(PermissionId))]
public class SessionPermission
{
    [Required]
    public Guid IdentityId { get; set; }


    [ForeignKey(nameof(PermissionId))]
    public Permission Permission { get; set; } = null!;

    [Required]
    public Guid PermissionId { get; set; }

    [ForeignKey(nameof(IdentityId))]
    public Session Session { get; set; } = null!;
}