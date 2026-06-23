using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Shuttle.Access.SqlServer.Models;

[Index(nameof(SessionId), nameof(TokenHash), IsUnique = true, Name = $"UX_{nameof(SessionToken)}_{nameof(SessionId)}_{nameof(TokenHash)}")]
public class SessionToken
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    [StringLength(64)]
    public string TokenHash { get; set; } = string.Empty;

    [Required]
    public DateTimeOffset DateRegistered { get; set; }

    [Required]
    public DateTimeOffset ExpiryDate { get; set; }

    [Required]
    [StringLength(200)]
    public string Application { get; set; } = "Access";

    public Session Session { get; set; } = null!;
}