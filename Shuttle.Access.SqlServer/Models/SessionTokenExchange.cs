using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(SessionTokenExchange), Schema = "access")]
public class SessionTokenExchange
{
    [Key]
    public Guid ExchangeToken { get; set; }

    [Required]
    public DateTimeOffset ExpiryDate { get; set; }

    [Required]
    public Guid SessionToken { get; set; }
}