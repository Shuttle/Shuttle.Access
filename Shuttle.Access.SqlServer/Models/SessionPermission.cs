using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.SqlServer.Models;

[PrimaryKey(nameof(SessionId), nameof(PermissionId))]
public class SessionPermission
{
    [Required]
    public Guid SessionId { get; set; }
    [Required]
    public Guid PermissionId { get; set; }

    public Permission Permission { get; set; } = null!;
    public Session Session { get; set; } = null!;
}