namespace Shuttle.Access;

public interface ISessionTokenExchangeRepository
{
    Task<SessionTokenExchange?> FindAsync(Guid exchangeToken, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid exchangeToken, CancellationToken cancellationToken = default);
    Task RemoveExpiredAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(SessionTokenExchange sessionTokenExchange, CancellationToken cancellationToken = default);
}