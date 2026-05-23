using System.Text.RegularExpressions;

namespace Shuttle.Access.WebApi.Contracts.v1;

public static class SessionExtensions
{
    extension(Session session)
    {
        public bool HasPermission(Guid tenantId, string requiredPermission)
        {
            ArgumentNullException.ThrowIfNull(session);
            ArgumentException.ThrowIfNullOrWhiteSpace(requiredPermission);

            return session.Permissions
                .Any(permission => permission.TenantId == tenantId && Regex.IsMatch(requiredPermission, $"^{Regex.Escape(permission.Name).Replace(@"\*", ".*")}$", RegexOptions.IgnoreCase));
        }
    }
}