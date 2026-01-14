using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(PermissionTenant), Schema = "access")]
[PrimaryKey(nameof(PermissionId), nameof(TenantId))]
public class PermissionTenant
{
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
}