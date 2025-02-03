using System;

namespace Shuttle.Access;

public class SessionTokenExchange
{
    public Guid SessionToken { get; }
    public Guid ExchangeToken { get; }
    public DateTime ExpiryDate { get; }

    public bool HasExpired => DateTime.UtcNow > ExpiryDate;

    public SessionTokenExchange(Guid exchangeToken, Guid sessionToken, DateTime expiryDate)
    {
        SessionToken = sessionToken;
        ExchangeToken = exchangeToken;
        ExpiryDate = expiryDate;
    }
}