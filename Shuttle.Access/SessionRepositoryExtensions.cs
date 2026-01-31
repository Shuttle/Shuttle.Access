using Shuttle.Access.Query;
using Shuttle.Core.Contract;

namespace Shuttle.Access;

public static class SessionRepositoryExtensions
{
    extension(ISessionRepository sessionRepository)
    {
        public async Task<Session?> FindAsync(SessionSpecification specification, CancellationToken cancellationToken = default)
        {
            var sessions = (await Guard.AgainstNull(sessionRepository).SearchAsync(specification, cancellationToken)).ToList();

            return sessions.Count > 1
                ? throw new ApplicationException(string.Format(Resources.SessionCountException, sessions.Count))
                : sessions.FirstOrDefault();
        }
    }
}