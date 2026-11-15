using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(IdentityAlternateName), Schema = "access")]
[PrimaryKey(nameof(IdentityId), nameof(Name))]
[Index(nameof(Name), IsUnique = true, Name = $"UX_{nameof(IdentityAlternateName)}_{nameof(Name)}")]
public class IdentityAlternateName
{
    public Identity Identity { get; set; } = null!;

    [Required]
    public Guid IdentityId { get; set; }

    [Required]
    [StringLength(320)]
    public string Name { get; set; } = string.Empty;
}
