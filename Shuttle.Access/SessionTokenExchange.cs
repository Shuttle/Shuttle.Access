namespace Shuttle.Access;

public class SessionTokenExchange
{
    public SessionTokenExchange(Guid exchangeToken, Guid sessionToken, DateTimeOffset expiryDate)
    {
        SessionToken = sessionToken;
        ExchangeToken = exchangeToken;
        ExpiryDate = expiryDate;
    }

    public Guid ExchangeToken { get; }
    public DateTimeOffset ExpiryDate { get; }

    public bool HasExpired => DateTimeOffset.UtcNow > ExpiryDate;
    public Guid SessionToken { get; }
}