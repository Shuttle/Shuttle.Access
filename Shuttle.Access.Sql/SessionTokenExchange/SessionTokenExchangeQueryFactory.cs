using System;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

public class SessionTokenExchangeQueryFactory : ISessionTokenExchangeQueryFactory
{
    public IQuery Save(SessionTokenExchange sessionTokenExchange)
    {
        Guard.AgainstNull(sessionTokenExchange);

        return new Query($@"
INSERT INTO [dbo].[SessionTokenExchange]
(
    ExchangeToken,
    SessionToken,
    ExpiryDate
)
VALUES
(
    @ExchangeToken,
    @SessionToken,
    @ExpiryDate
)
")
            .AddParameter(Columns.ExchangeToken, sessionTokenExchange.ExchangeToken)
            .AddParameter(Columns.SessionToken, sessionTokenExchange.SessionToken)
            .AddParameter(Columns.ExpiryDate, sessionTokenExchange.ExpiryDate);
    }

    public IQuery Find(Guid exchangeToken)
    {
        return new Query(@"
SELECT
    ExchangeToken,
    SessionToken,
    ExpiryDate
FROM
    [dbo].[SessionTokenExchange]
WHERE
    ExchangeToken = @ExchangeToken
")
            .AddParameter(Columns.ExchangeToken, exchangeToken);
    }

    public IQuery Remove(Guid exchangeToken)
    {
        return new Query($@"DELETE FROM [dbo].[SessionTokenExchange] WHERE ExchangeToken = @ExchangeToken")
            .AddParameter(Columns.ExchangeToken, exchangeToken);
    }

    public IQuery RemoveExpired()
    {
        return new Query("DELETE FROM [dbo].[SessionTokenExchange] WHERE ExpiryDate < GETUTCDATE()");
    }
}