using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(Session), Schema = "access")]
[Index(nameof(TokenHash), IsUnique = true, Name = $"UX_{nameof(Session)}_{nameof(TokenHash)}")]
[Index(nameof(IdentityId), IsUnique = true, Name = $"UX_{nameof(Session)}_{nameof(IdentityId)}")]
public class Session
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public DateTimeOffset DateRegistered { get; set; }

    [Required]
    public DateTimeOffset ExpiryDate { get; set; }

    public Identity Identity { get; set; } = null!;

    [Required]
    public Guid IdentityId { get; set; }

    public ICollection<SessionPermission> SessionPermissions { get; set; } = [];

    [Required]
    [StringLength(64)]
    public string TokenHash { get; set; } = string.Empty;
}