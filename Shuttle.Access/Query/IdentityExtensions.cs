using static Shuttle.Access.AccessPermissions;

namespace Shuttle.Access.Query;

public static class IdentityExtensions
{
    extension(Identity identity)
    {
        public bool HasPermission(Guid tenantId, string requiredPermission)
        {
            ArgumentNullException.ThrowIfNull(identity);
            ArgumentException.ThrowIfNullOrWhiteSpace(requiredPermission);

            return identity.Roles.Where(item => item.TenantId == tenantId).SelectMany(item => item.Permissions).Any(item => item.IsSatisfiedBy(Identities.Register));
        }
    }
}