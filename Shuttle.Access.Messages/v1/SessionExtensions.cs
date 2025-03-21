using Shuttle.Core.Contract;

namespace Shuttle.Access.Messages.v1;

public static class SessionExtensions
{
    public static bool HasPermission(this Session session, string permission)
    {
        return Guard.AgainstNull(session).Permissions.Contains(Guard.AgainstNullOrEmptyString(permission));
    }
}