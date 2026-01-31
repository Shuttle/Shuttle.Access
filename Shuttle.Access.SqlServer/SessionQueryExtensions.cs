using Shuttle.Access.Query;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public static class SessionQueryExtensions
{
    extension(ISessionQuery sessionQuery)
    {
        public async ValueTask<bool> ContainsAsync(SessionSpecification sessionSpecification, CancellationToken cancellationToken = default)
        {
            return await Guard.AgainstNull(sessionQuery).CountAsync(sessionSpecification, cancellationToken) > 0;
        }
    }
}