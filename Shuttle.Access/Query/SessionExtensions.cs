namespace Shuttle.Access.Query;

public static class SessionExtensions
{
    extension(Session session)
    {
        public bool HasPermission(Guid tenantId, string requiredPermission)
        {
            ArgumentNullException.ThrowIfNull(session);
            ArgumentException.ThrowIfNullOrWhiteSpace(requiredPermission);

            return session.Permissions.Any(permission => permission.TenantId == tenantId && permission.IsSatisfiedBy(requiredPermission));
        }
    }
}