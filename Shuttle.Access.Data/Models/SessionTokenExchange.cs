using System.ComponentModel.DataAnnotations;

namespace Shuttle.Access.Data.Models;

public class SessionTokenExchange
{
    [Key]
    public Guid ExchangeToken { get; set; }

    [Required]
    public Guid SessionToken { get; set; }

    [Required]
    public DateTimeOffset ExpiryDate { get; set; }
}