using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(Session), Schema = "access")]
[Index(nameof(Token), IsUnique = true, Name = $"UX_{nameof(Session)}_{nameof(Token)}")]
[Index(nameof(IdentityId), IsUnique = true, Name = $"UX_{nameof(Session)}_{nameof(IdentityId)}")]
[Index(nameof(IdentityName), IsUnique = true, Name = $"UX_{nameof(Session)}_{nameof(IdentityName)}")]
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

    public Guid? TenantId { get; set; }
    
    [Required]
    [StringLength(320)]
    public string IdentityName { get; set; } = string.Empty;

    public ICollection<SessionPermission> SessionPermissions { get; set; } = [];

    public Tenant? Tenant { get; set; }

    [Required]
    public byte[] Token { get; set; } = new byte[32];
}