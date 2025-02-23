using System;

namespace Shuttle.Access;

public class SessionTokenExchange
{
    public Guid SessionToken { get; }
    public Guid ExchangeToken { get; }
    public DateTimeOffset ExpiryDate { get; }

    public bool HasExpired => DateTimeOffset.UtcNow > ExpiryDate;

    public SessionTokenExchange(Guid exchangeToken, Guid sessionToken, DateTimeOffset expiryDate)
    {
        SessionToken = sessionToken;
        ExchangeToken = exchangeToken;
        ExpiryDate = expiryDate;
    }
}