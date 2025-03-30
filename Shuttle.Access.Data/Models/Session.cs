using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Access.Data.Models;

[Index(nameof(Token), IsUnique = true, Name = $"UX_{nameof(Session)}_{nameof(Token)}")]
[Index(nameof(IdentityName), IsUnique = true, Name = $"UX_{nameof(Session)}_{nameof(IdentityName)}")]
public class Session
{
    [Key]
    public Guid IdentityId { get; set; }

    [Required]
    [StringLength(320)]
    public string IdentityName { get; set; } = string.Empty;

    [Required]
    public byte[] Token { get; set; } = new byte[32];

    [Required]
    [StringLength(320)]
    public string RegisteredBy { get; set; } = string.Empty;

    [Required]
    public DateTimeOffset DateRegistered { get; set; }

    [Required]
    public DateTimeOffset ExpiryDate { get; set; }
}