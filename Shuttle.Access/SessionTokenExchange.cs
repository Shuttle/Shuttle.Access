namespace Shuttle.Access;

public class SessionTokenExchange(Guid exchangeToken, Guid sessionToken, DateTimeOffset expiryDate)
{
    public Guid ExchangeToken { get; } = exchangeToken;
    public DateTimeOffset ExpiryDate { get; } = expiryDate;

    public bool HasExpired => DateTimeOffset.UtcNow > ExpiryDate;
    public Guid SessionToken { get; } = sessionToken;
}