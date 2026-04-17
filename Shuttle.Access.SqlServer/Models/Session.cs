using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(Session), Schema = "access")]
[Index(nameof(Token), IsUnique = true, Name = $"UX_{nameof(Session)}_{nameof(Token)}")]
[Index(nameof(IdentityId), nameof(TenantId), IsUnique = true, Name = $"UX_{nameof(Session)}_{nameof(IdentityId)}_{nameof(TenantId)}")]
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

    public Guid TenantId { get; set; }

    public ICollection<SessionPermission> SessionPermissions { get; set; } = [];

    public Tenant Tenant { get; set; } = null!;

    [Required]
    public byte[] Token { get; set; } = new byte[32];
}