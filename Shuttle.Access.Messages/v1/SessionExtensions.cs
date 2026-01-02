using System.Text.RegularExpressions;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Messages.v1;

public static class SessionExtensions
{
    public static bool HasPermission(this Session session, string requiredPermission)
    {
        Guard.AgainstEmpty(requiredPermission);

        return Guard.AgainstNull(session).Permissions
            .Any(permission =>
                Regex.IsMatch(requiredPermission, $"^{Regex.Escape(permission).Replace(@"\*", ".*")}$", RegexOptions.IgnoreCase));
    }
}