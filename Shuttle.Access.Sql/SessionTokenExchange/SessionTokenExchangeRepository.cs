using System;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

public class SessionTokenExchangeRepository : ISessionTokenExchangeRepository
{
    private readonly ISessionTokenExchangeQueryFactory _sessionTokenExchangeQueryFactory;
    private readonly IDatabaseContextService _databaseContextService;

    public SessionTokenExchangeRepository(IDatabaseContextService databaseContextService, ISessionTokenExchangeQueryFactory sessionTokenExchangeQueryFactory)
    {
        _databaseContextService = Guard.AgainstNull(databaseContextService);
        _sessionTokenExchangeQueryFactory = Guard.AgainstNull(sessionTokenExchangeQueryFactory);
    }

    public async Task SaveAsync(SessionTokenExchange sessionTokenExchange, CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_sessionTokenExchangeQueryFactory.Save(sessionTokenExchange), cancellationToken);
    }

    public async Task<SessionTokenExchange?> FindAsync(Guid exchangeToken, CancellationToken cancellationToken = default)
    {
        var row = await _databaseContextService.Active.GetRowAsync(_sessionTokenExchangeQueryFactory.Find(exchangeToken), cancellationToken);

        if (row == null)
        {
            return null;
        }

        return new(
            Columns.ExchangeToken.Value(row),
            Columns.SessionToken.Value(row),
            Columns.ExpiryDate.Value(row));
    }

    public async Task RemoveAsync(Guid exchangeToken, CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_sessionTokenExchangeQueryFactory.Remove(exchangeToken), cancellationToken);
    }

    public async Task RemoveExpiredAsync(CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_sessionTokenExchangeQueryFactory.RemoveExpired(), cancellationToken);
    }
}