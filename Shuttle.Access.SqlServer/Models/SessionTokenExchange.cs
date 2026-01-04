using System.ComponentModel.DataAnnotations;

namespace Shuttle.Access.SqlServer.Models;

public class SessionTokenExchange
{
    [Key]
    public Guid ExchangeToken { get; set; }

    [Required]
    public DateTimeOffset ExpiryDate { get; set; }

    [Required]
    public Guid SessionToken { get; set; }
}