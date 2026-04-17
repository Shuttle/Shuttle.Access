using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(IdentityTenant), Schema = "access")]
[PrimaryKey(nameof(IdentityId), nameof(TenantId))]
public class IdentityTenant
{
    public Identity Identity { get; set; } = null!;
    public Guid IdentityId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public Guid TenantId { get; set; }
}