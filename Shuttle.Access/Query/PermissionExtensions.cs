using System.Text.RegularExpressions;

namespace Shuttle.Access.Query;

public static class PermissionExtensions
{
    extension(Permission permission)
    {
        public bool IsSatisfiedBy(string requiredPermission)
        {
            ArgumentNullException.ThrowIfNull(permission);
            ArgumentException.ThrowIfNullOrWhiteSpace(requiredPermission);

            return Regex.IsMatch(requiredPermission, $"^{Regex.Escape(permission.Name).Replace(@"\*", ".*")}$", RegexOptions.IgnoreCase);
        }
    }
}