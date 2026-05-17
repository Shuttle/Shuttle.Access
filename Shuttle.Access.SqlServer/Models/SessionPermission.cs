using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(SessionPermission), Schema = "access")]
[PrimaryKey(nameof(SessionId), nameof(TenantId), nameof(PermissionId))]
public class SessionPermission
{
    public Permission Permission { get; set; } = null!;

    [Required]
    public Guid PermissionId { get; set; }

    public Session Session { get; set; } = null!;

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid TenantId { get; set; }
}