using System.Text.RegularExpressions;

namespace Shuttle.Access.WebApi.Contracts.v1;

public static class SessionExtensions
{
    extension(Session session)
    {
        public bool HasPermission(string requiredPermission)
        {
            ArgumentNullException.ThrowIfNull(session);
            ArgumentException.ThrowIfNullOrWhiteSpace(requiredPermission);

            return session.Permissions
                .Any(permission =>
                    Regex.IsMatch(requiredPermission, $"^{Regex.Escape(permission.Name).Replace(@"\*", ".*")}$", RegexOptions.IgnoreCase));
        }
    }
}