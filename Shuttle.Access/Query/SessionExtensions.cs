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

        public bool HasExpired(TimeSpan sessionRenewalTolerance, string application)
        {
            var token = session.FindSessionToken(application);
            var expiryDate = DateTimeOffset.UtcNow.Subtract(sessionRenewalTolerance);

            return session.ExpiryDate < expiryDate ||
                   (token != null && token.ExpiryDate < expiryDate);
        }

        public Session.SessionToken? FindSessionToken(string application)
        {
            return session.Tokens.FirstOrDefault(item => item.Application.Equals(application, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}