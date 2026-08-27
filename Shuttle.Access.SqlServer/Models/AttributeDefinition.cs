using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(AttributeDefinition), Schema = "access")]
[PrimaryKey(nameof(TenantId), nameof(Id))]
[Index(nameof(TenantId), nameof(Name), IsUnique = true, Name = $"UX_{nameof(AttributeDefinition)}_{nameof(TenantId)}_{nameof(Name)}")]
[Index(nameof(Id), IsUnique = true, Name = $"UX_{nameof(AttributeDefinition)}_{nameof(Id)}")]
public class AttributeDefinition
{
    [Required]
    public int Cardinality { get; set; }

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public ICollection<IdentityAttribute> IdentityAttributes { get; set; } = [];

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid TenantId { get; set; }

    [ForeignKey(nameof(TenantId))]
    public Tenant Tenant { get; set; } = null!;

    [Required]
    public int Type { get; set; }
}
