using Shuttle.Core.Contract;

namespace Shuttle.Access.Messages.v1;

public static class SessionResponseExtensions
{
    extension(SessionResponse sessionResponse)
    {
        public bool IsSuccessResult() => Guard.AgainstNull(sessionResponse).Result.Equals("Registered", StringComparison.InvariantCultureIgnoreCase);
    }
}