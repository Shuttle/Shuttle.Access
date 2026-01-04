using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class SessionTokenExchangeRepository(AccessDbContext accessDbContext) : ISessionTokenExchangeRepository
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async Task SaveAsync(SessionTokenExchange sessionTokenExchange, CancellationToken cancellationToken = default)
    {
        _accessDbContext.SessionTokenExchange.Add(new()
        {
            SessionToken = sessionTokenExchange.SessionToken,
            ExchangeToken = sessionTokenExchange.ExchangeToken,
            ExpiryDate = sessionTokenExchange.ExpiryDate
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<SessionTokenExchange?> FindAsync(Guid exchangeToken, CancellationToken cancellationToken = default)
    {
        var model = await _accessDbContext.SessionTokenExchange.AsNoTracking().Where(e => e.ExchangeToken == exchangeToken).SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (model == null)
        {
            return null;
        }

        return new(model.ExchangeToken, model.SessionToken, model.ExpiryDate);
    }

    public async Task RemoveAsync(Guid exchangeToken, CancellationToken cancellationToken = default)
    {
        var model = await _accessDbContext.SessionTokenExchange.AsNoTracking().Where(e => e.ExchangeToken == exchangeToken).SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (model == null)
        {
            return;
        }

        _accessDbContext.SessionTokenExchange.Remove(model);

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveExpiredAsync(CancellationToken cancellationToken = default)
    {
        await _accessDbContext.Database.ExecuteSqlAsync($"DELETE FROM [dbo].[SessionTokenExchange] WHERE ExpiryDate < SYSUTCDATETIME()", cancellationToken: cancellationToken);
    }
}