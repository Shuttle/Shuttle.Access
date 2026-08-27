using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(IdentityAttribute), Schema = "access")]
[Index(nameof(TenantId), nameof(IdentityId), nameof(AttributeDefinitionId), Name = $"IX_{nameof(IdentityAttribute)}_{nameof(TenantId)}_{nameof(IdentityId)}_{nameof(AttributeDefinitionId)}")]
public class IdentityAttribute
{
    public AttributeDefinition AttributeDefinition { get; set; } = null!;

    [Required]
    public Guid AttributeDefinitionId { get; set; }

    [Key]
    public Guid Id { get; set; }

    public Identity Identity { get; set; } = null!;

    [Required]
    public Guid IdentityId { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [StringLength(2000)]
    public string Value { get; set; } = string.Empty;
}
