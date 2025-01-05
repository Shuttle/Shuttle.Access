using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access;

public interface ISessionTokenExchangeRepository
{
    Task SaveAsync(SessionTokenExchange sessionTokenExchange, CancellationToken cancellationToken = default);
    Task<SessionTokenExchange?> FindAsync(Guid exchangeToken, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid exchangeToken, CancellationToken cancellationToken = default);
    Task RemoveExpiredAsync(CancellationToken cancellationToken = default);
}